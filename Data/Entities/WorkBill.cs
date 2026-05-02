namespace Work_Dashboard.Data.Entities;

public enum BillStatus { SUBMITTED, PASSED, PAID, REJECTED }

public class WorkBill
{
    public int        BillId           { get; set; }
    public int        WorkId           { get; set; }
    public string     BillNumber       { get; set; } = "";
    public DateOnly?  BillDate         { get; set; }
    public decimal    GrossAmount      { get; set; }
    public decimal    Deductions       { get; set; }
    public decimal    NetAmount        { get; set; }
    public decimal    CumulativeBilled { get; set; }
    public BillStatus Status           { get; set; } = BillStatus.SUBMITTED;
    public string?    Remarks          { get; set; }
    public string?    PdfUrl           { get; set; }
    public int?       CreatedBy        { get; set; }
    public DateTime   CreatedAt        { get; set; } = DateTime.UtcNow;
    public DateTime?  UpdatedAt        { get; set; }

    public Work?     Work              { get; set; }
    public Engineer? CreatedByEngineer { get; set; }
}
