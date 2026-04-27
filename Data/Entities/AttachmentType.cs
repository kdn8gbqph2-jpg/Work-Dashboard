namespace Work_Dashboard.Data.Entities;

public class AttachmentType
{
    public int TypeId { get; set; }
    public string TypeCode { get; set; } = "";
    public string TypeName { get; set; } = "";

    public ICollection<AttachmentCategory> AttachmentCategories { get; set; } = [];
    public ICollection<Attachment> Attachments { get; set; } = [];
}
