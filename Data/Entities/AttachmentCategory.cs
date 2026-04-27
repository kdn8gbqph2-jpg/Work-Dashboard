namespace Work_Dashboard.Data.Entities;

public class AttachmentCategory
{
    public int CatId { get; set; }
    public string CatCode { get; set; } = "";
    public string CatName { get; set; } = "";
    public int? AllowedTypeId { get; set; }

    public AttachmentType? AllowedType { get; set; }
    public ICollection<Attachment> Attachments { get; set; } = [];
}
