namespace Work_Dashboard.Data.Entities;

public class BudgetHead
{
    public int BudgetHeadId { get; set; }
    public string BudgetCode { get; set; } = "";
    public string BudgetName { get; set; } = "";
    public decimal AvailableFund { get; set; }
    public decimal AmountSpent { get; set; }
    public decimal RemainingAmount { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
