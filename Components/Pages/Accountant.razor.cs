using ClosedXML.Excel;
using Microsoft.JSInterop;
using Work_Dashboard.Data.Entities;

namespace Work_Dashboard.Components.Pages;

public partial class Accountant
{
    private static readonly string[] ExportHeaders =
    {
        "#", "File No.", "Work Name", "Status",
        "Progress(%)", "Fin Progress(%)",
        "Sanctioned(L)", "WO Amt(L)",
        "JEN", "AEN", "XEN", "Contractor", "Location", "Remarks"
    };

    private string LatestRemark(int workId) =>
        LatestRemarksMap.TryGetValue(workId, out var r) ? r.Content : "";

    private static string CsvVal(string? s) =>
        s == null ? "" : "\"" + s.Replace("\"", "\"\"") + "\"";

    private async Task ExportCsv()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine(string.Join(",", ExportHeaders));
        int i = 1;
        foreach (var w in FilteredWorks)
            sb.AppendLine(
                $"{i++},{CsvVal(w.WorkCode)},{CsvVal(w.WorkName)},{w.Status}," +
                $"{w.ProgressPercent ?? 0},{w.FinancialProgressPercent ?? 0}," +
                $"{w.SanctionedAmount ?? 0},{w.AgreementAmount ?? 0}," +
                $"{CsvVal(w.AssignedJen?.Name)},{CsvVal(w.AssignedAen?.Name)},{CsvVal(w.AssignedExen?.Name)}," +
                $"{CsvVal(w.ContractorName)},{CsvVal(w.Location)},{CsvVal(LatestRemark(w.WorkId))}");
        await JS.InvokeVoidAsync("bdaDownloadText", $"BDA_Works_{DateTime.Now:yyyyMMdd}.csv", sb.ToString());
    }

    private async Task ExportExcel()
    {
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("Works");

        for (int col = 0; col < ExportHeaders.Length; col++)
        {
            var cell = ws.Cell(1, col + 1);
            cell.Value = ExportHeaders[col];
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#16a34a");
            cell.Style.Font.Bold      = true;
            cell.Style.Font.FontColor = XLColor.White;
        }

        int row = 2, idx = 1;
        foreach (var w in FilteredWorks)
        {
            ws.Cell(row, 1).Value  = idx++;
            ws.Cell(row, 2).Value  = w.WorkCode    ?? "";
            ws.Cell(row, 3).Value  = w.WorkName;
            ws.Cell(row, 4).Value  = w.Status.ToString();
            ws.Cell(row, 5).Value  = (double)(w.ProgressPercent          ?? 0);
            ws.Cell(row, 6).Value  = (double)(w.FinancialProgressPercent ?? 0);
            ws.Cell(row, 7).Value  = (double)(w.SanctionedAmount         ?? 0);
            ws.Cell(row, 8).Value  = (double)(w.AgreementAmount          ?? 0);
            ws.Cell(row, 9).Value  = w.AssignedJen?.Name  ?? "";
            ws.Cell(row, 10).Value = w.AssignedAen?.Name  ?? "";
            ws.Cell(row, 11).Value = w.AssignedExen?.Name ?? "";
            ws.Cell(row, 12).Value = w.ContractorName     ?? "";
            ws.Cell(row, 13).Value = w.Location           ?? "";
            ws.Cell(row, 14).Value = LatestRemark(w.WorkId);
            row++;
        }

        ws.Columns().AdjustToContents();
        foreach (var c in ws.ColumnsUsed())
            if (c.Width > 50) c.Width = 50;

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        await JS.InvokeVoidAsync("bdaDownloadBase64",
            $"BDA_Works_{DateTime.Now:yyyyMMdd}.xlsx",
            Convert.ToBase64String(ms.ToArray()));
    }

    private async Task ExportPdf()
    {
        var headers = new[]
        {
            "#", "File No.", "Work Name", "Status",
            "Phys%", "Fin%", "Sanc(L)", "WO(L)",
            "JEN", "AEN", "Contractor", "Remarks"
        };
        int idx = 1;
        var rows = FilteredWorks.Select(w => new string[]
        {
            (idx++).ToString(),
            w.WorkCode ?? "",
            w.WorkName,
            w.Status.ToString(),
            $"{w.ProgressPercent ?? 0}%",
            $"{w.FinancialProgressPercent ?? 0}%",
            $"{w.SanctionedAmount ?? 0:F2}",
            $"{w.AgreementAmount  ?? 0:F2}",
            w.AssignedJen?.Name  ?? "",
            w.AssignedAen?.Name  ?? "",
            w.ContractorName     ?? "",
            LatestRemark(w.WorkId)
        }).ToArray();

        await JS.InvokeVoidAsync("bdaExportPdf",
            "BDA Works Report",
            "Bharatpur Development Authority",
            headers, rows);
    }

    static string StatusLabel(WorkStatus s) => s switch
    {
        WorkStatus.ONGOING   => "Ongoing",
        WorkStatus.COMPLETED => "Completed",
        WorkStatus.STALLED   => "Stalled",
        WorkStatus.CANCELLED => "Cancelled",
        _                    => s.ToString()
    };

    static string ProgColor(decimal pct) => pct switch
    {
        >= 100 => "prog-green",
        >= 75  => "prog-blue",
        >= 40  => "prog-yellow",
        _      => "prog-red"
    };
}
