namespace Work_Dashboard.Data.Entities;

public class WorkProgressLog
{
    public int LogId { get; set; }
    public int WorkId { get; set; }
    public decimal? ProgressPercent { get; set; }
    public string? Remarks { get; set; }
    public int? LoggedBy { get; set; }
    public DateTime LoggedAt { get; set; } = DateTime.UtcNow;

    public Work Work { get; set; } = null!;
    public Engineer? LoggedByEngineer { get; set; }
}
