using Microsoft.EntityFrameworkCore;
using Work_Dashboard.Data.Entities;

namespace Work_Dashboard.Data;

public class BdaDbContext(DbContextOptions<BdaDbContext> options) : DbContext(options)
{
    public DbSet<Engineer>        Engineers        => Set<Engineer>();
    public DbSet<WorkCategory>    WorkCategories   => Set<WorkCategory>();
    public DbSet<FundSource>      FundSources      => Set<FundSource>();
    public DbSet<Work>            Works            => Set<Work>();
    public DbSet<WorkProgressLog> WorkProgressLogs => Set<WorkProgressLog>();
    public DbSet<WorkRemark>      WorkRemarks      => Set<WorkRemark>();
    public DbSet<WorkFile>            WorkFiles            => Set<WorkFile>();
    public DbSet<AuditLog>            AuditLogs            => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // ── engineers ────────────────────────────────────────────
        mb.Entity<Engineer>(e =>
        {
            e.ToTable("engineers");
            e.HasKey(x => x.EngineerId);
            e.Property(x => x.EngineerId).HasColumnName("engineer_id");
            e.Property(x => x.Name).HasColumnName("name").HasMaxLength(150);
            e.Property(x => x.Username).HasColumnName("username").HasMaxLength(50);
            e.Property(x => x.PasswordHash).HasColumnName("password_hash").HasMaxLength(255);
            e.Property(x => x.Role).HasColumnName("role").HasConversion<string>();
            e.Property(x => x.Mobile).HasColumnName("mobile").HasMaxLength(15);
            e.Property(x => x.Email).HasColumnName("email").HasMaxLength(100);
            e.Property(x => x.IsActive).HasColumnName("is_active");
            e.Property(x => x.IsDeleted).HasColumnName("is_deleted");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            e.HasIndex(x => x.Username).IsUnique();
        });

        // ── work_categories ──────────────────────────────────────
        mb.Entity<WorkCategory>(e =>
        {
            e.ToTable("work_categories");
            e.HasKey(x => x.CategoryId);
            e.Property(x => x.CategoryId).HasColumnName("category_id");
            e.Property(x => x.CategoryCode).HasColumnName("category_code").HasMaxLength(20);
            e.Property(x => x.CategoryName).HasColumnName("category_name").HasMaxLength(100);
            e.Property(x => x.IsActive).HasColumnName("is_active");
            e.HasIndex(x => x.CategoryCode).IsUnique();
        });

        // ── fund_sources ─────────────────────────────────────────
        mb.Entity<FundSource>(e =>
        {
            e.ToTable("fund_sources");
            e.HasKey(x => x.FundSourceId);
            e.Property(x => x.FundSourceId).HasColumnName("fund_source_id");
            e.Property(x => x.SourceCode).HasColumnName("source_code").HasMaxLength(20);
            e.Property(x => x.SourceName).HasColumnName("source_name").HasMaxLength(100);
            e.Property(x => x.IsActive).HasColumnName("is_active");
            e.HasIndex(x => x.SourceCode).IsUnique();
        });

        // ── works ────────────────────────────────────────────────
        mb.Entity<Work>(e =>
        {
            e.ToTable("works");
            e.HasKey(x => x.WorkId);
            e.Property(x => x.WorkId).HasColumnName("work_id");
            e.Property(x => x.WorkName).HasColumnName("work_name").HasMaxLength(1000);
            e.Property(x => x.WorkCode).HasColumnName("work_code").HasMaxLength(50);
            e.Property(x => x.CategoryId).HasColumnName("category_id");
            e.Property(x => x.FundSourceId).HasColumnName("fund_source_id");
            e.Property(x => x.Department).HasColumnName("department").HasMaxLength(50);
            e.Property(x => x.AssignedJenId).HasColumnName("assigned_jen_id");
            e.Property(x => x.AssignedAenId).HasColumnName("assigned_aen_id");
            e.Property(x => x.AssignedExenId).HasColumnName("assigned_exen_id");
            e.Property(x => x.ContractorName).HasColumnName("contractor_name").HasMaxLength(200);
            e.Property(x => x.ContractorMobile).HasColumnName("contractor_mobile").HasMaxLength(15);
            e.Property(x => x.SanctionedAmount).HasColumnName("sanctioned_amount").HasPrecision(12, 4);
            e.Property(x => x.AgreementAmount).HasColumnName("agreement_amount").HasPrecision(12, 4);
            e.Property(x => x.Expenditure).HasColumnName("expenditure").HasPrecision(12, 4);
            e.Property(x => x.ProgressPercent).HasColumnName("progress_percent").HasPrecision(5, 2);
            e.Property(x => x.FinancialProgressPercent).HasColumnName("financial_progress_percent").HasPrecision(5, 2);
            e.Property(x => x.PaymentStatus).HasColumnName("payment_status").HasMaxLength(100);
            e.Property(x => x.Status).HasColumnName("status").HasConversion<string>();
            e.Property(x => x.IsAnnualContract).HasColumnName("is_annual_contract");
            e.Property(x => x.IsScheme).HasColumnName("is_scheme");
            e.Property(x => x.IsCmBudget).HasColumnName("is_cm_budget");
            e.Property(x => x.StartDate).HasColumnName("start_date");
            e.Property(x => x.ExpectedCompletion).HasColumnName("expected_completion");
            e.Property(x => x.ActualCompletion).HasColumnName("actual_completion");
            e.Property(x => x.Location).HasColumnName("location").HasMaxLength(200);
            e.Property(x => x.WardNumber).HasColumnName("ward_number").HasMaxLength(20);
            e.Property(x => x.Remarks).HasColumnName("remarks");
            e.Property(x => x.IsDeleted).HasColumnName("is_deleted");
            e.Property(x => x.CreatedBy).HasColumnName("created_by");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            e.HasIndex(x => x.WorkCode).IsUnique();

            e.HasOne(x => x.Category).WithMany(x => x.Works).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.FundSource).WithMany(x => x.Works).HasForeignKey(x => x.FundSourceId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.AssignedJen).WithMany(x => x.JenWorks).HasForeignKey(x => x.AssignedJenId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.AssignedAen).WithMany(x => x.AenWorks).HasForeignKey(x => x.AssignedAenId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.AssignedExen).WithMany(x => x.ExenWorks).HasForeignKey(x => x.AssignedExenId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.CreatedByEngineer).WithMany().HasForeignKey(x => x.CreatedBy).OnDelete(DeleteBehavior.Restrict);
        });

        // ── work_remarks ─────────────────────────────────────────
        mb.Entity<WorkRemark>(e =>
        {
            e.ToTable("work_remarks");
            e.HasKey(x => x.RemarkId);
            e.Property(x => x.RemarkId).HasColumnName("remark_id");
            e.Property(x => x.WorkId).HasColumnName("work_id");
            e.Property(x => x.Content).HasColumnName("content").HasColumnType("text");
            e.Property(x => x.AuthorName).HasColumnName("author_name").HasMaxLength(150);
            e.Property(x => x.AuthorId).HasColumnName("author_id");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.Property(x => x.EditedByName).HasColumnName("edited_by_name").HasMaxLength(200);
            e.Property(x => x.EditedById).HasColumnName("edited_by_id");
            e.Property(x => x.EditedAt).HasColumnName("edited_at");

            e.HasOne(x => x.Work).WithMany().HasForeignKey(x => x.WorkId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Author).WithMany().HasForeignKey(x => x.AuthorId).OnDelete(DeleteBehavior.SetNull);
        });

        // ── work_progress_log ────────────────────────────────────
        mb.Entity<WorkProgressLog>(e =>
        {
            e.ToTable("work_progress_log");
            e.HasKey(x => x.LogId);
            e.Property(x => x.LogId).HasColumnName("log_id");
            e.Property(x => x.WorkId).HasColumnName("work_id");
            e.Property(x => x.ProgressPercent).HasColumnName("progress_percent").HasPrecision(5, 2);
            e.Property(x => x.Remarks).HasColumnName("remarks");
            e.Property(x => x.LoggedBy).HasColumnName("logged_by");
            e.Property(x => x.LoggedAt).HasColumnName("logged_at");

            e.HasOne(x => x.Work).WithMany(x => x.ProgressLogs).HasForeignKey(x => x.WorkId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.LoggedByEngineer).WithMany(x => x.ProgressLogs).HasForeignKey(x => x.LoggedBy).OnDelete(DeleteBehavior.Restrict);
        });

        // ── files ────────────────────────────────────────────────
        mb.Entity<WorkFile>(e =>
        {
            e.ToTable("files");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.WorkId).HasColumnName("work_id");
            e.Property(x => x.FileType).HasColumnName("file_type").HasConversion<string>().HasMaxLength(10);
            e.Property(x => x.FileUrl).HasColumnName("file_url").HasMaxLength(1000);
            e.Property(x => x.UploadedBy).HasColumnName("uploaded_by");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");

            e.HasOne(x => x.Work).WithMany().HasForeignKey(x => x.WorkId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.UploadedByEngineer).WithMany().HasForeignKey(x => x.UploadedBy).OnDelete(DeleteBehavior.SetNull);
        });

        // ── audit_log ────────────────────────────────────────────
        mb.Entity<AuditLog>(e =>
        {
            e.ToTable("audit_log");
            e.HasKey(x => x.LogId);
            e.Property(x => x.LogId).HasColumnName("log_id");
            e.Property(x => x.TableName).HasColumnName("table_name").HasMaxLength(50);
            e.Property(x => x.RecordId).HasColumnName("record_id");
            e.Property(x => x.Action).HasColumnName("action").HasConversion<string>();
            e.Property(x => x.ChangedBy).HasColumnName("changed_by");
            e.Property(x => x.ChangedAt).HasColumnName("changed_at");
            e.Property(x => x.Details).HasColumnName("details").HasColumnType("json");
            e.HasOne(x => x.ChangedByEngineer).WithMany().HasForeignKey(x => x.ChangedBy).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
