namespace Work_Dashboard.Data.Entities;

public enum AuditAction { INSERT, UPDATE, DELETE }

public class AuditLog
{
    public long LogId { get; set; }
    public string TableName { get; set; } = "";
    public int RecordId { get; set; }
    public AuditAction Action { get; set; }
    public int? ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public string? Details { get; set; }

    public Engineer? ChangedByEngineer { get; set; }
}
