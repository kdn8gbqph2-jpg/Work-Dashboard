namespace Work_Dashboard.Data.Entities;

/// <summary>
/// A timestamped remark/comment added by a user against a specific work.
/// AuthorName is stored as a string so remarks are preserved even if the
/// engineer account is later deleted.
/// </summary>
public class WorkRemark
{
    public int RemarkId { get; set; }
    public int WorkId { get; set; }
    public string Content { get; set; } = "";
    public string AuthorName { get; set; } = "";   // denormalised – always kept
    public int? AuthorId { get; set; }              // nullable FK to engineers
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string? EditedByName { get; set; }       // null = never edited
    public int? EditedById { get; set; }
    public DateTime? EditedAt { get; set; }

    public Work Work { get; set; } = null!;
    public Engineer? Author { get; set; }
}
