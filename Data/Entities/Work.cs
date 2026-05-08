namespace Work_Dashboard.Data.Entities;

public enum WorkStatus { ONGOING, COMPLETED, STALLED, CANCELLED }

public class Work
{
    public int WorkId { get; set; }
    public string WorkName { get; set; } = "";
    public string? WorkCode { get; set; }
    public int? CategoryId { get; set; }
    public int? FundSourceId { get; set; }
    public string? Department { get; set; }
    public int? AssignedJenId { get; set; }
    public int? AssignedAenId { get; set; }
    public int? AssignedExenId { get; set; }
    public string? ContractorName { get; set; }
    public string? ContractorMobile { get; set; }
    public decimal? SanctionedAmount { get; set; }   // A&F Amount
    public string?  AnfNo   { get; set; }
    public DateOnly? AnfDate { get; set; }
    public decimal? AgreementAmount { get; set; }     // Work Order Amount
    public string?  WorkOrderNo   { get; set; }
    public DateOnly? WorkOrderDate { get; set; }
    public decimal? Expenditure { get; set; }
    public decimal? ProgressPercent { get; set; }
    public decimal? FinancialProgressPercent { get; set; }
    public string? PaymentStatus { get; set; }
    public WorkStatus Status { get; set; } = WorkStatus.ONGOING;
    public bool IsAnnualContract { get; set; } = false;
    public bool IsScheme { get; set; } = false;
    public bool IsCmBudget { get; set; } = false;
    public DateOnly? StartDate { get; set; }
    public DateOnly? ExpectedCompletion { get; set; }
    public DateOnly? ActualCompletion { get; set; }
    public string? Location { get; set; }
    public string? WardNumber { get; set; }
    public string? Remarks { get; set; }
    public bool IsDeleted { get; set; } = false;
    public int? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public WorkCategory? Category { get; set; }
    public FundSource? FundSource { get; set; }
    public Engineer? AssignedJen { get; set; }
    public Engineer? AssignedAen { get; set; }
    public Engineer? AssignedExen { get; set; }
    public Engineer? CreatedByEngineer { get; set; }
    public ICollection<WorkProgressLog> ProgressLogs { get; set; } = [];
}
