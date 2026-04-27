namespace Work_Dashboard.Data.Entities;

public enum ColumnDataType { TEXT, NUMBER, DATE, BOOLEAN, DROPDOWN }

public class CustomColumn
{
    public int ColumnId { get; set; }
    public int TableId { get; set; }
    public string ColumnName { get; set; } = "";
    public ColumnDataType DataType { get; set; } = ColumnDataType.TEXT;
    public int DisplayOrder { get; set; } = 0;
    public bool IsRequired { get; set; } = false;
    public string? DropdownOptions { get; set; }
    public bool IsDeleted { get; set; } = false;

    public CustomTable Table { get; set; } = null!;
    public ICollection<CustomCell> Cells { get; set; } = [];
}
