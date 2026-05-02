using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using Work_Dashboard.Data;
using Work_Dashboard.Data.Entities;

namespace Work_Dashboard.Components.Shared;

public partial class WorkBillsPanel
{
    // ── Parameters ────────────────────────────────────────────────
    [Parameter] public int      WorkId          { get; set; }
    [Parameter] public decimal? AgreementAmount { get; set; }
    [Parameter] public decimal? SanctionedAmount { get; set; }
    [Parameter] public bool     CanEdit         { get; set; } = true;
    [Parameter] public int?     CurrentUserId   { get; set; }
    [Parameter] public string   CurrentUserName { get; set; } = "";

    // ── Injected services ─────────────────────────────────────────
    [Inject] IDbContextFactory<BdaDbContext> DbFactory { get; set; } = default!;
    [Inject] IWebHostEnvironment             Env       { get; set; } = default!;
    [Inject] IJSRuntime                      JS        { get; set; } = default!;

    // ── State ─────────────────────────────────────────────────────
    List<WorkBill> Bills      = [];
    bool           ShowForm   = false;
    bool           Saving     = false;
    string         BillError  = "";

    // Form fields for new bill
    string     FBillNumber  = "";
    DateOnly?  FBillDate    = null;
    decimal    FGross       = 0;
    decimal    FDeductions  = 0;
    decimal    FNet         = 0;
    string     FStatusStr   = "SUBMITTED";
    string     FRemarks     = "";
    IBrowserFile? FPdfFile  = null;
    string     FPdfName     = "";

    // Edit state
    int        EditingBillId = 0;

    // ── Computed ──────────────────────────────────────────────────
    decimal TotalBilled  => Bills.Sum(b => b.NetAmount);
    decimal WoAmt        => AgreementAmount ?? 0;
    decimal Remaining    => WoAmt - TotalBilled;
    double  BilledPct    => WoAmt > 0 ? Math.Min(100, (double)(TotalBilled / WoAmt * 100)) : 0;

    static readonly string[] BillColors =
        { "#3b82f6", "#f97316", "#10b981", "#8b5cf6", "#ef4444", "#eab308" };

    string BillColor(int index) => BillColors[index % BillColors.Length];

    double SegPct(WorkBill b) =>
        WoAmt > 0 ? Math.Min(100, (double)(b.NetAmount / WoAmt * 100)) : 0;

    // ── Lifecycle ─────────────────────────────────────────────────
    protected override async Task OnParametersSetAsync()
    {
        await LoadBills();
    }

    async Task LoadBills()
    {
        await using var db = await DbFactory.CreateDbContextAsync();
        Bills = await db.WorkBills
            .Where(b => b.WorkId == WorkId)
            .OrderBy(b => b.CreatedAt)
            .ToListAsync();
    }

    // ── Add Bill ──────────────────────────────────────────────────
    void OpenAddForm()
    {
        EditingBillId = 0;
        FBillNumber   = $"{Bills.Count + 1}";
        FBillDate     = DateOnly.FromDateTime(DateTime.Today);
        FGross        = 0; FDeductions = 0; FNet = 0;
        FStatusStr    = "SUBMITTED";
        FRemarks      = "";
        FPdfFile      = null;
        FPdfName      = "";
        BillError     = "";
        ShowForm      = true;
    }

    void OpenEditForm(WorkBill b)
    {
        EditingBillId = b.BillId;
        FBillNumber   = b.BillNumber;
        FBillDate     = b.BillDate;
        FGross        = b.GrossAmount;
        FDeductions   = b.Deductions;
        FNet          = b.NetAmount;
        FStatusStr    = b.Status.ToString();
        FRemarks      = b.Remarks ?? "";
        FPdfFile      = null;
        FPdfName      = b.PdfUrl != null ? Path.GetFileName(b.PdfUrl) : "";
        BillError     = "";
        ShowForm      = true;
    }

    void CancelForm() { ShowForm = false; BillError = ""; }

    void RecomputeNet()
    {
        FNet = FGross - FDeductions;
        if (FNet < 0) FNet = 0;
    }

    void OnPdfSelected(InputFileChangeEventArgs e)
    {
        FPdfFile = e.File;
        FPdfName = e.File.Name;
    }

    async Task SaveBill()
    {
        BillError = "";
        if (string.IsNullOrWhiteSpace(FBillNumber)) { BillError = "Bill Number is required."; return; }

        RecomputeNet();
        Saving = true;

        string? pdfUrl = null;

        // Upload PDF if a new file is chosen
        if (FPdfFile != null)
        {
            var mime = FPdfFile.ContentType;
            if (!string.Equals(mime, "application/pdf", StringComparison.OrdinalIgnoreCase))
            { BillError = "Only PDF files are allowed."; Saving = false; return; }
            if (FPdfFile.Size > 20 * 1024 * 1024)
            { BillError = "PDF must be under 20 MB."; Saving = false; return; }

            var relDir  = $"uploads/works/{WorkId}/bills";
            var absDir  = Path.Combine(Env.WebRootPath, relDir.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(absDir);
            var fileName = Guid.NewGuid().ToString("N") + ".pdf";
            var absPath  = Path.Combine(absDir, fileName);
            await using (var fs = File.Create(absPath))
            await using (var src = FPdfFile.OpenReadStream(20 * 1024 * 1024))
                await src.CopyToAsync(fs);
            pdfUrl = "/" + relDir + "/" + fileName;
        }

        await using var db = await DbFactory.CreateDbContextAsync();
        var status = Enum.Parse<BillStatus>(FStatusStr);

        if (EditingBillId == 0)
        {
            // New bill — compute cumulative
            var cumulative = TotalBilled + FNet;
            db.WorkBills.Add(new WorkBill
            {
                WorkId           = WorkId,
                BillNumber       = FBillNumber.Trim(),
                BillDate         = FBillDate,
                GrossAmount      = FGross,
                Deductions       = FDeductions,
                NetAmount        = FNet,
                CumulativeBilled = cumulative,
                Status           = status,
                Remarks          = string.IsNullOrWhiteSpace(FRemarks) ? null : FRemarks.Trim(),
                PdfUrl           = pdfUrl,
                CreatedBy        = CurrentUserId,
                CreatedAt        = DateTime.UtcNow
            });
        }
        else
        {
            var existing = await db.WorkBills.FindAsync(EditingBillId);
            if (existing != null)
            {
                existing.BillNumber  = FBillNumber.Trim();
                existing.BillDate    = FBillDate;
                existing.GrossAmount = FGross;
                existing.Deductions  = FDeductions;
                existing.NetAmount   = FNet;
                existing.Status      = status;
                existing.Remarks     = string.IsNullOrWhiteSpace(FRemarks) ? null : FRemarks.Trim();
                existing.UpdatedAt   = DateTime.UtcNow;
                if (pdfUrl != null) existing.PdfUrl = pdfUrl;
            }
        }

        await db.SaveChangesAsync();
        ShowForm = false;
        Saving   = false;
        await LoadBills();
    }

    async Task DeleteBill(WorkBill bill)
    {
        await using var db = await DbFactory.CreateDbContextAsync();
        var b = await db.WorkBills.FindAsync(bill.BillId);
        if (b != null)
        {
            if (!string.IsNullOrEmpty(b.PdfUrl))
            {
                var abs = Path.Combine(Env.WebRootPath,
                    b.PdfUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (File.Exists(abs)) try { File.Delete(abs); } catch { /* ignore */ }
            }
            db.WorkBills.Remove(b);
            await db.SaveChangesAsync();
        }
        await LoadBills();
    }

    // ── Helpers ───────────────────────────────────────────────────
    static string StatusBadge(BillStatus s) => s switch
    {
        BillStatus.SUBMITTED => "bg-primary",
        BillStatus.PASSED    => "bg-warning text-dark",
        BillStatus.PAID      => "bg-success",
        BillStatus.REJECTED  => "bg-danger",
        _                    => "bg-secondary"
    };

    static string StatusLabel(BillStatus s) => s switch
    {
        BillStatus.SUBMITTED => "Submitted",
        BillStatus.PASSED    => "Passed",
        BillStatus.PAID      => "Paid",
        BillStatus.REJECTED  => "Rejected",
        _                    => s.ToString()
    };

    static string Fmt(decimal d) => d.ToString("N2");
}
