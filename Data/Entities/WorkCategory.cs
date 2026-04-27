namespace Work_Dashboard.Data.Entities;

public class WorkCategory
{
    public int CategoryId { get; set; }
    public string CategoryCode { get; set; } = "";
    public string CategoryName { get; set; } = "";
    public bool IsActive { get; set; } = true;

    public ICollection<Work> Works { get; set; } = [];
}
