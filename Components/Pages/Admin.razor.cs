using ClosedXML.Excel;
using Microsoft.JSInterop;
using Work_Dashboard.Data.Entities;
using Work_Dashboard.Services;

namespace Work_Dashboard.Components.Pages;

public partial class Admin
{
    // ── Column definitions ────────────────────────────────────
    record ExportCol(string Key, string Label, string ShortLabel, bool Required = false, bool DefaultOn = true);

    private static readonly ExportCol[] AllCols =
    [
        new("serial",      "#",                     "#",          Required: true,  DefaultOn: true),
        new("file_no",     "File No.",               "File No.",   DefaultOn: true),
        new("work_name",   "Work Name",              "Work Name",  Required: true,  DefaultOn: true),
        new("status",      "Status",                 "Status",     Required: true,  DefaultOn: true),
        new("category",    "Category",               "Category",   DefaultOn: true),
        new("fund_source", "Fund Source",            "Fund Src",   DefaultOn: true),
        new("department",  "Department",             "Dept",       DefaultOn: false),
        new("start_date",  "Start Date",             "Start",      DefaultOn: false),
        new("exp_comp",    "Expected Completion",    "Exp.Comp",   DefaultOn: false),
        new("act_comp",    "Actual Completion",      "Act.Comp",   DefaultOn: false),
        new("progress",    "Physical Progress (%)",  "Phys%",      DefaultOn: true),
        new("fin_progress","Financial Progress (%)","Fin%",        DefaultOn: false),
        new("sanc_amt",    "Sanctioned Amt (Lakhs)", "Sanc(L)",    DefaultOn: true),
        new("agr_amt",     "Workorder Amt (Lakhs)",  "WO(L)",      DefaultOn: false),
        new("jen",         "JEN",                   "JEN",        DefaultOn: true),
        new("aen",         "AEN",                   "AEN",        DefaultOn: true),
        new("xen",         "XEN",                   "XEN",        DefaultOn: true),
        new("contractor",  "Contractor",            "Contractor", DefaultOn: true),
        new("cont_mobile", "Contractor Mobile",     "Cont.Mob",   DefaultOn: false),
        new("location",    "Location",              "Location",   DefaultOn: false),
        new("ward",        "Ward No.",              "Ward",       DefaultOn: false),
        new("annual",      "Annual Contract",       "Annual",     DefaultOn: false),
        new("scheme",      "Scheme",                "Scheme",     DefaultOn: false),
        new("cm_budget",   "CM Budget",             "CM Budg",    DefaultOn: false),
        new("remarks",     "Remarks",               "Remarks",    DefaultOn: true),
    ];

    private const string LocalStorageKey = "bda_report_cols";

    // Keys currently selected — defaults populated immediately so exports
    // work even before OnAfterRenderAsync has a chance to read localStorage.
    private HashSet<string> _selectedCols =
        AllCols.Where(c => c.DefaultOn).Select(c => c.Key).ToHashSet();
    private bool _colsLoaded = false;
    bool ShowColPicker = false;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && !_colsLoaded)
        {
            _colsLoaded = true;
            var saved = await JS.InvokeAsync<string?>("bdaGetItem", LocalStorageKey);
            if (!string.IsNullOrEmpty(saved))
            {
                var keys = saved.Split(',', StringSplitOptions.RemoveEmptyEntries).ToHashSet();
                foreach (var c in AllCols.Where(c => c.Required)) keys.Add(c.Key);
                _selectedCols = keys;
                StateHasChanged();
            }
        }

        if (Section == "overview" && !IsLoading && !_chartsReady)
        {
            _chartsReady = true;
            await RenderOverviewCharts();
        }
    }

    private async Task SaveColSelection()
    {
        await JS.InvokeVoidAsync("bdaSetItem", LocalStorageKey, string.Join(",", _selectedCols));
        ShowColPicker = false;
    }

    // Ordered cols for export (preserves AllCols order, only selected)
    private IEnumerable<ExportCol> ActiveCols =>
        AllCols.Where(c => _selectedCols.Contains(c.Key));

    // ── Value resolver ────────────────────────────────────────
    private string ColValue(Work w, string key, int serial) => key switch
    {
        "serial"       => serial.ToString(),
        "file_no"      => w.WorkCode        ?? "",
        "work_name"    => w.WorkName,
        "status"       => w.Status.ToString(),
        "category"     => w.Category?.CategoryName ?? "",
        "fund_source"  => w.FundSource?.SourceName ?? "",
        "department"   => w.Department      ?? "",
        "start_date"   => w.StartDate?.ToString("dd/MM/yyyy") ?? "",
        "exp_comp"     => w.ExpectedCompletion?.ToString("dd/MM/yyyy") ?? "",
        "act_comp"     => w.ActualCompletion?.ToString("dd/MM/yyyy") ?? "",
        "progress"     => w.ProgressPercent.HasValue ? $"{w.ProgressPercent:F1}" : "",
        "fin_progress" => w.FinancialProgressPercent.HasValue ? $"{w.FinancialProgressPercent:F1}" : "",
        "sanc_amt"     => w.SanctionedAmount.HasValue ? $"{w.SanctionedAmount:F2}" : "",
        "agr_amt"      => w.AgreementAmount.HasValue  ? $"{w.AgreementAmount:F2}"  : "",
        "jen"          => w.AssignedJen?.Name  ?? "",
        "aen"          => w.AssignedAen?.Name  ?? "",
        "xen"          => w.AssignedExen?.Name ?? "",
        "contractor"   => w.ContractorName  ?? "",
        "cont_mobile"  => w.ContractorMobile ?? "",
        "location"     => w.Location        ?? "",
        "ward"         => w.WardNumber       ?? "",
        "annual"       => w.IsAnnualContract ? "Yes" : "No",
        "scheme"       => w.IsScheme         ? "Yes" : "No",
        "cm_budget"    => w.IsCmBudget       ? "Yes" : "No",
        "remarks"      => LatestRemark(w.WorkId),
        _              => ""
    };

    private string LatestRemark(int workId) =>
        LatestRemarksMap.TryGetValue(workId, out var r) ? r.Content : "";

    // CsvVal is in WorkHelpers; keep a local alias for brevity.
    private static string CsvVal(string? s) => WorkHelpers.CsvVal(s);

    // ── Export methods ────────────────────────────────────────
    private async Task ExportCsv()
    {
        var cols = ActiveCols.ToList();
        var sb   = new System.Text.StringBuilder();
        sb.AppendLine(string.Join(",", cols.Select(c => CsvVal(c.Label))));
        int i = 1;
        foreach (var w in FilteredWorks)
            sb.AppendLine(string.Join(",", cols.Select(c => CsvVal(ColValue(w, c.Key, i)))));
        await JS.InvokeVoidAsync("bdaDownloadText", $"BDA_Works_{DateTime.Now:yyyyMMdd}.csv", sb.ToString());
    }

    private async Task ExportExcel()
    {
        var cols = ActiveCols.ToList();
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("Works");

        for (int col = 0; col < cols.Count; col++)
        {
            var cell = ws.Cell(1, col + 1);
            cell.Value = cols[col].Label;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#FF8C00");
            cell.Style.Font.Bold            = true;
            cell.Style.Font.FontColor       = XLColor.White;
        }

        int row = 2, idx = 1;
        foreach (var w in FilteredWorks)
        {
            for (int col = 0; col < cols.Count; col++)
                ws.Cell(row, col + 1).Value = ColValue(w, cols[col].Key, idx);
            idx++; row++;
        }

        ws.Columns().AdjustToContents();
        // Cap very wide columns
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
        var cols = ActiveCols.ToList();
        var headers = cols.Select(c => c.ShortLabel).ToArray();
        int idx = 1;
        var rows = FilteredWorks
            .Select(w => cols.Select(c => ColValue(w, c.Key, idx++)).ToArray())
            .ToArray();

        await JS.InvokeVoidAsync("bdaExportPdf",
            "BDA Works Report",
            "Bharatpur Development Authority",
            headers,
            rows);
    }
}
