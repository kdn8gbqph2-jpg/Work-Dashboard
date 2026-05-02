using Work_Dashboard.Data.Entities;

namespace Work_Dashboard.Services;

/// <summary>
/// Static helpers shared across Admin, Jen, and Accountant pages.
/// Centralises label/colour logic and CSV escaping so they never drift apart.
/// </summary>
public static class WorkHelpers
{
    // ── Work status ────────────────────────────────────────────────────────
    public static string StatusLabel(WorkStatus s) => s switch
    {
        WorkStatus.ONGOING   => "Ongoing",
        WorkStatus.COMPLETED => "Completed",
        WorkStatus.STALLED   => "Stalled",
        WorkStatus.CANCELLED => "Cancelled",
        _                    => s.ToString()
    };

    // ── Progress colour (returns CSS class for progress-fill elements) ─────
    /// <summary>Maps a progress percentage to one of the shared .progress-fill-* classes.</summary>
    public static string ProgColor(decimal pct) => pct switch
    {
        >= 75 => "progress-fill-green",
        >= 40 => "progress-fill-orange",
        _     => "progress-fill-red"
    };

    // ── Financial year ────────────────────────────────────────────────────
    /// <summary>Returns "YYYY-YY" financial year for a given date (April–March).</summary>
    public static string FinYear(DateOnly d) =>
        d.Month >= 4
            ? $"{d.Year}-{d.Year + 1 - 2000:D2}"
            : $"{d.Year - 1}-{d.Year - 2000:D2}";

    // ── CSV escaping ──────────────────────────────────────────────────────
    /// <summary>Wraps a value in double-quotes and escapes any embedded quotes.</summary>
    public static string CsvVal(string? s) =>
        s is null ? "\"\"" : "\"" + s.Replace("\"", "\"\"") + "\"";

    // ── Bill status ───────────────────────────────────────────────────────
    public static string BillStatusBadge(BillStatus s) => s switch
    {
        BillStatus.SUBMITTED => "bg-primary",
        BillStatus.PASSED    => "bg-warning text-dark",
        BillStatus.PAID      => "bg-success",
        BillStatus.REJECTED  => "bg-danger",
        _                    => "bg-secondary"
    };

    public static string BillStatusLabel(BillStatus s) => s switch
    {
        BillStatus.SUBMITTED => "Submitted",
        BillStatus.PASSED    => "Passed",
        BillStatus.PAID      => "Paid",
        BillStatus.REJECTED  => "Rejected",
        _                    => s.ToString()
    };
}
