namespace Work_Dashboard.Data.Entities;

public enum AttachmentParentType { WORK, PROGRESS_LOG, CUSTOM_ROW }

public class Attachment
{
    public int AttachmentId { get; set; }
    public AttachmentParentType ParentType { get; set; }
    public int ParentId { get; set; }
    public int? TypeId { get; set; }
    public int? CatId { get; set; }
    public string FileName { get; set; } = "";
    public string? DriveFileId { get; set; }
    public string? DriveViewUrl { get; set; }
    public long? FileSizeBytes { get; set; }
    public string? MimeType { get; set; }
    public int? UploadedBy { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;

    public AttachmentType? Type { get; set; }
    public AttachmentCategory? Category { get; set; }
    public Engineer? UploadedByEngineer { get; set; }
}
