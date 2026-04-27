namespace Work_Dashboard.Data.Entities;

public class DriveFolder
{
    public int FolderId { get; set; }
    public int WorkId { get; set; }
    public string? RootFolderId { get; set; }
    public string? PhotosFolderId { get; set; }
    public string? PdfsFolderId { get; set; }
    public string? VideosFolderId { get; set; }
    public string? DrawingsFolderId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Work Work { get; set; } = null!;
}
