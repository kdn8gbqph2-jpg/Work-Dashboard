namespace Work_Dashboard.Data.Entities;

public enum EngineerRole { ADMIN, JEN, AEN, XEN }

public class Engineer
{
    public int EngineerId { get; set; }
    public string Name { get; set; } = "";
    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public EngineerRole Role { get; set; } = EngineerRole.JEN;
    public string? Mobile { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Work> JenWorks { get; set; } = [];
    public ICollection<Work> AenWorks { get; set; } = [];
    public ICollection<Work> ExenWorks { get; set; } = [];
    public ICollection<WorkProgressLog> ProgressLogs { get; set; } = [];
}
