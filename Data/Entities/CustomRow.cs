namespace Work_Dashboard.Data.Entities;

public class CustomRow
{
    public int RowId { get; set; }
    public int TableId { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;

    public CustomTable Table { get; set; } = null!;
    public Engineer? CreatedByEngineer { get; set; }
    public ICollection<CustomCell> Cells { get; set; } = [];
}
