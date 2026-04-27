namespace Work_Dashboard.Data.Entities;

public class FundSource
{
    public int FundSourceId { get; set; }
    public string SourceCode { get; set; } = "";
    public string SourceName { get; set; } = "";
    public bool IsActive { get; set; } = true;

    public ICollection<Work> Works { get; set; } = [];
}
