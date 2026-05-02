using ClosedXML.Excel;
using Microsoft.JSInterop;
using Work_Dashboard.Data.Entities;
using Work_Dashboard.Services;

namespace Work_Dashboard.Components.Pages;

public partial class Jen
{
    // ── Export helpers ────────────────────────────────────────
    // CsvVal is in WorkHelpers; keep a local alias for brevity.
    private static string CsvVal(string? s) => WorkHelpers.CsvVal(s);

    private static readonly string[] ExportHeaders =
    {
        "#", "File No.", "Work Name", "Status",
        "Progress (%)", "Sanctioned Amt (Lakhs)",
        "JEN", "AEN", "XEN", "Contractor", "Location", "Remarks"
    };

    private static readonly string[] PdfHeaders =
    {
        "#", "File No.", "Work Name", "Status",
        "Progress%", "Amt(L)", "JEN", "AEN", "XEN", "Contractor", "Remarks"
    };

    private string LatestRemark(int workId) =>
        LatestRemarksMap.TryGetValue(workId, out var r) ? r.Content : "";

    private async Task ExportCsv()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine(string.Join(",", ExportHeaders));
        int i = 1;
        foreach (var w in FilteredWorks)
            sb.AppendLine(
                $"{i++},{CsvVal(w.WorkCode)},{CsvVal(w.WorkName)}," +
                $"{w.Status},{w.ProgressPercent ?? 0},{w.SanctionedAmount ?? 0}," +
                $"{CsvVal(w.AssignedJen?.Name)},{CsvVal(w.AssignedAen?.Name)}," +
                $"{CsvVal(w.AssignedExen?.Name)},{CsvVal(w.ContractorName)},{CsvVal(w.Location)}," +
                $"{CsvVal(LatestRemark(w.WorkId))}");
        await JS.InvokeVoidAsync("bdaDownloadText", $"BDA_Works_{DateTime.Now:yyyyMMdd}.csv", sb.ToString());
    }

    private async Task ExportExcel()
    {
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("Works");

        // Header row
        for (int col = 0; col != ExportHeaders.Length; col++)
        {
            var cell = ws.Cell(1, col + 1);
            cell.Value = ExportHeaders[col];
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#FF8C00");
            cell.Style.Font.Bold            = true;
            cell.Style.Font.FontColor       = XLColor.White;
        }

        // Data rows
        int row = 2, idx = 1;
        foreach (var w in FilteredWorks)
        {
            ws.Cell(row, 1).Value  = idx++;
            ws.Cell(row, 2).Value  = w.WorkCode ?? "";
            ws.Cell(row, 3).Value  = w.WorkName;
            ws.Cell(row, 4).Value  = w.Status.ToString();
            ws.Cell(row, 5).Value  = (double)(w.ProgressPercent ?? 0);
            ws.Cell(row, 6).Value  = (double)(w.SanctionedAmount ?? 0);
            ws.Cell(row, 7).Value  = w.AssignedJen?.Name  ?? "";
            ws.Cell(row, 8).Value  = w.AssignedAen?.Name  ?? "";
            ws.Cell(row, 9).Value  = w.AssignedExen?.Name ?? "";
            ws.Cell(row, 10).Value = w.ContractorName      ?? "";
            ws.Cell(row, 11).Value = w.Location            ?? "";
            ws.Cell(row, 12).Value = LatestRemark(w.WorkId);
            row++;
        }

        ws.Columns().AdjustToContents();
        ws.Column(12).Width = 40; // Remarks column wider
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        await JS.InvokeVoidAsync("bdaDownloadBase64",
            $"BDA_Works_{DateTime.Now:yyyyMMdd}.xlsx",
            Convert.ToBase64String(ms.ToArray()));
    }

    private async Task ExportPdf()
    {
        var rows = FilteredWorks.Select((w, i) => new string[]
        {
            (i + 1).ToString(),
            w.WorkCode ?? "",
            w.WorkName,
            w.Status.ToString(),
            $"{w.ProgressPercent ?? 0}%",
            $"{w.SanctionedAmount ?? 0:F2}",
            w.AssignedJen?.Name  ?? "",
            w.AssignedAen?.Name  ?? "",
            w.AssignedExen?.Name ?? "",
            w.ContractorName     ?? "",
            LatestRemark(w.WorkId)
        }).ToArray();

        await JS.InvokeVoidAsync("bdaExportPdf",
            "BDA Works Report",
            "Bharatpur Development Authority",
            PdfHeaders,
            rows);
    }

    // ── Hindi transliteration ─────────────────────────────────
    bool HindiMode = false;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        try { await JS.InvokeVoidAsync("bdaHindi.attach", "jenRemarkInput"); } catch { }
    }

    async Task ToggleHindiMode()
    {
        HindiMode = !HindiMode;
        try { await JS.InvokeVoidAsync("bdaHindi.setMode", "jenRemarkInput", HindiMode); } catch { }
    }
}
