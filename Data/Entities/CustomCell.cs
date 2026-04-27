namespace Work_Dashboard.Data.Entities;

public class CustomCell
{
    public long CellId { get; set; }
    public int RowId { get; set; }
    public int ColumnId { get; set; }
    public string? Value { get; set; }

    public CustomRow Row { get; set; } = null!;
    public CustomColumn Column { get; set; } = null!;
}
