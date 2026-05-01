using Microsoft.EntityFrameworkCore;
using Work_Dashboard.Data;
using Work_Dashboard.Data.Entities;

namespace Work_Dashboard.Services;

public class FileUploadService
{
    private readonly IDbContextFactory<BdaDbContext> _dbFactory;
    private readonly IWebHostEnvironment _env;

    public const int    MaxPhotosPerWork = 10;
    public const long   MaxPhotoBytes    = 5  * 1024 * 1024;   // 5 MB
    public const long   MaxPdfBytes      = 20 * 1024 * 1024;   // 20 MB

    private static readonly string[] AllowedImageMimes = { "image/jpeg", "image/png", "image/webp" };
    private const string PdfMime = "application/pdf";

    public FileUploadService(IDbContextFactory<BdaDbContext> dbFactory, IWebHostEnvironment env)
    {
        _dbFactory = dbFactory;
        _env       = env;
    }

    /// <summary>Upload a single file. Returns the saved WorkFile or throws.</summary>
    public async Task<WorkFile> SaveAsync(
        int workId,
        WorkFileType type,
        string originalName,
        string mimeType,
        long sizeBytes,
        Stream content,
        int? uploadedBy)
    {
        // ── validate ─────────────────────────────────────────────
        if (type == WorkFileType.image)
        {
            if (!AllowedImageMimes.Contains(mimeType.ToLowerInvariant()))
                throw new InvalidOperationException("Only JPEG, PNG, WEBP images are allowed.");
            if (sizeBytes > MaxPhotoBytes)
                throw new InvalidOperationException($"Image exceeds {MaxPhotoBytes / 1024 / 1024} MB limit.");
        }
        else
        {
            if (!string.Equals(mimeType, PdfMime, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Only PDF files are allowed.");
            if (sizeBytes > MaxPdfBytes)
                throw new InvalidOperationException($"PDF exceeds {MaxPdfBytes / 1024 / 1024} MB limit.");
        }

        // ── filesystem path ──────────────────────────────────────
        var subDir   = type == WorkFileType.image ? "photos" : "pdfs";
        var relDir   = $"uploads/works/{workId}/{subDir}";
        var absDir   = Path.Combine(_env.WebRootPath, relDir.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(absDir);

        var ext      = Path.GetExtension(originalName);
        var fileName = $"{Guid.NewGuid():N}{ext}";
        var absPath  = Path.Combine(absDir, fileName);
        var relUrl   = $"/{relDir}/{fileName}";

        await using (var fs = File.Create(absPath))
            await content.CopyToAsync(fs);

        // ── DB row ───────────────────────────────────────────────
        await using var db = await _dbFactory.CreateDbContextAsync();
        var row = new WorkFile
        {
            WorkId     = workId,
            FileType   = type,
            FileUrl    = relUrl,
            UploadedBy = uploadedBy,
            CreatedAt  = DateTime.UtcNow
        };
        db.WorkFiles.Add(row);
        await db.SaveChangesAsync();
        return row;
    }

    /// <summary>Delete a file by id (DB row + physical file).</summary>
    public async Task DeleteAsync(int fileId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var f = await db.WorkFiles.FindAsync(fileId);
        if (f is null) return;

        // physical file
        var rel = f.FileUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var abs = Path.Combine(_env.WebRootPath, rel);
        if (File.Exists(abs))
        {
            try { File.Delete(abs); } catch { /* ignore */ }
        }

        db.WorkFiles.Remove(f);
        await db.SaveChangesAsync();
    }

    /// <summary>Count current photos for a work.</summary>
    public async Task<int> PhotoCountAsync(int workId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.WorkFiles.CountAsync(f => f.WorkId == workId && f.FileType == WorkFileType.image);
    }

    /// <summary>List all files for a work, newest first.</summary>
    public async Task<List<WorkFile>> ListForWorkAsync(int workId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.WorkFiles
            .Where(f => f.WorkId == workId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }
}
