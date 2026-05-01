namespace Work_Dashboard.Data.Entities;

public enum WorkFileType { pdf, image }

public class WorkFile
{
    public int         Id          { get; set; }
    public int         WorkId      { get; set; }
    public WorkFileType FileType   { get; set; }
    public string      FileUrl     { get; set; } = "";
    public int?        UploadedBy  { get; set; }
    public DateTime    CreatedAt   { get; set; } = DateTime.UtcNow;

    public Work?     Work                  { get; set; }
    public Engineer? UploadedByEngineer    { get; set; }
}
