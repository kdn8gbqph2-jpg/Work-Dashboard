namespace Work_Dashboard.Data.Entities;

public class CustomTable
{
    public int TableId { get; set; }
    public string TableName { get; set; } = "";
    public string? Description { get; set; }
    public int? WorkId { get; set; }
    public int? CreatedBy { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Work? Work { get; set; }
    public Engineer? CreatedByEngineer { get; set; }
    public ICollection<CustomColumn> Columns { get; set; } = [];
    public ICollection<CustomRow> Rows { get; set; } = [];
}
