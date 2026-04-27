using Microsoft.EntityFrameworkCore;
using Work_Dashboard.Data.Entities;

namespace Work_Dashboard.Data;

public class BdaDbContext(DbContextOptions<BdaDbContext> options) : DbContext(options)
{
    public DbSet<Engineer> Engineers => Set<Engineer>();
    public DbSet<WorkCategory> WorkCategories => Set<WorkCategory>();
    public DbSet<FundSource> FundSources => Set<FundSource>();
    public DbSet<Work> Works => Set<Work>();
    public DbSet<WorkProgressLog> WorkProgressLogs => Set<WorkProgressLog>();
    public DbSet<AttachmentType> AttachmentTypes => Set<AttachmentType>();
    public DbSet<AttachmentCategory> AttachmentCategories => Set<AttachmentCategory>();
    public DbSet<Attachment> Attachments => Set<Attachment>();
    public DbSet<DriveFolder> DriveFolders => Set<DriveFolder>();
    public DbSet<GoogleOAuthToken> GoogleOAuthTokens => Set<GoogleOAuthToken>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<CustomTable> CustomTables => Set<CustomTable>();
    public DbSet<CustomColumn> CustomColumns => Set<CustomColumn>();
    public DbSet<CustomRow> CustomRows => Set<CustomRow>();
    public DbSet<CustomCell> CustomCells => Set<CustomCell>();

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

        // ── attachment_types ─────────────────────────────────────
        mb.Entity<AttachmentType>(e =>
        {
            e.ToTable("attachment_types");
            e.HasKey(x => x.TypeId);
            e.Property(x => x.TypeId).HasColumnName("type_id");
            e.Property(x => x.TypeCode).HasColumnName("type_code").HasMaxLength(20);
            e.Property(x => x.TypeName).HasColumnName("type_name").HasMaxLength(50);
            e.HasIndex(x => x.TypeCode).IsUnique();
        });

        // ── attachment_categories ────────────────────────────────
        mb.Entity<AttachmentCategory>(e =>
        {
            e.ToTable("attachment_categories");
            e.HasKey(x => x.CatId);
            e.Property(x => x.CatId).HasColumnName("cat_id");
            e.Property(x => x.CatCode).HasColumnName("cat_code").HasMaxLength(30);
            e.Property(x => x.CatName).HasColumnName("cat_name").HasMaxLength(100);
            e.Property(x => x.AllowedTypeId).HasColumnName("allowed_type_id");
            e.HasIndex(x => x.CatCode).IsUnique();
            e.HasOne(x => x.AllowedType).WithMany(x => x.AttachmentCategories).HasForeignKey(x => x.AllowedTypeId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── attachments ──────────────────────────────────────────
        mb.Entity<Attachment>(e =>
        {
            e.ToTable("attachments");
            e.HasKey(x => x.AttachmentId);
            e.Property(x => x.AttachmentId).HasColumnName("attachment_id");
            e.Property(x => x.ParentType).HasColumnName("parent_type").HasConversion<string>();
            e.Property(x => x.ParentId).HasColumnName("parent_id");
            e.Property(x => x.TypeId).HasColumnName("type_id");
            e.Property(x => x.CatId).HasColumnName("cat_id");
            e.Property(x => x.FileName).HasColumnName("file_name").HasMaxLength(255);
            e.Property(x => x.DriveFileId).HasColumnName("drive_file_id").HasMaxLength(150);
            e.Property(x => x.DriveViewUrl).HasColumnName("drive_view_url").HasMaxLength(1000);
            e.Property(x => x.FileSizeBytes).HasColumnName("file_size_bytes");
            e.Property(x => x.MimeType).HasColumnName("mime_type").HasMaxLength(100);
            e.Property(x => x.UploadedBy).HasColumnName("uploaded_by");
            e.Property(x => x.UploadedAt).HasColumnName("uploaded_at");
            e.Property(x => x.IsDeleted).HasColumnName("is_deleted");
            e.HasIndex(x => new { x.ParentType, x.ParentId });

            e.HasOne(x => x.Type).WithMany(x => x.Attachments).HasForeignKey(x => x.TypeId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Category).WithMany(x => x.Attachments).HasForeignKey(x => x.CatId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.UploadedByEngineer).WithMany().HasForeignKey(x => x.UploadedBy).OnDelete(DeleteBehavior.Restrict);
        });

        // ── drive_folders ────────────────────────────────────────
        mb.Entity<DriveFolder>(e =>
        {
            e.ToTable("drive_folders");
            e.HasKey(x => x.FolderId);
            e.Property(x => x.FolderId).HasColumnName("folder_id");
            e.Property(x => x.WorkId).HasColumnName("work_id");
            e.Property(x => x.RootFolderId).HasColumnName("root_folder_id").HasMaxLength(150);
            e.Property(x => x.PhotosFolderId).HasColumnName("photos_folder_id").HasMaxLength(150);
            e.Property(x => x.PdfsFolderId).HasColumnName("pdfs_folder_id").HasMaxLength(150);
            e.Property(x => x.VideosFolderId).HasColumnName("videos_folder_id").HasMaxLength(150);
            e.Property(x => x.DrawingsFolderId).HasColumnName("drawings_folder_id").HasMaxLength(150);
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.HasIndex(x => x.WorkId).IsUnique();

            e.HasOne(x => x.Work).WithOne(x => x.DriveFolder).HasForeignKey<DriveFolder>(x => x.WorkId).OnDelete(DeleteBehavior.Cascade);
        });

        // ── google_oauth_token ───────────────────────────────────
        mb.Entity<GoogleOAuthToken>(e =>
        {
            e.ToTable("google_oauth_token");
            e.HasKey(x => x.TokenId);
            e.Property(x => x.TokenId).HasColumnName("token_id").ValueGeneratedNever();
            e.Property(x => x.ClientId).HasColumnName("client_id").HasMaxLength(200);
            e.Property(x => x.ClientSecret).HasColumnName("client_secret").HasMaxLength(200);
            e.Property(x => x.RefreshToken).HasColumnName("refresh_token");
            e.Property(x => x.AccessToken).HasColumnName("access_token");
            e.Property(x => x.TokenExpiry).HasColumnName("token_expiry");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at");
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

        // ── custom_tables ────────────────────────────────────────
        mb.Entity<CustomTable>(e =>
        {
            e.ToTable("custom_tables");
            e.HasKey(x => x.TableId);
            e.Property(x => x.TableId).HasColumnName("table_id");
            e.Property(x => x.TableName).HasColumnName("table_name").HasMaxLength(200);
            e.Property(x => x.Description).HasColumnName("description");
            e.Property(x => x.WorkId).HasColumnName("work_id");
            e.Property(x => x.CreatedBy).HasColumnName("created_by");
            e.Property(x => x.IsDeleted).HasColumnName("is_deleted");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at");

            e.HasOne(x => x.Work).WithMany(x => x.CustomTables).HasForeignKey(x => x.WorkId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.CreatedByEngineer).WithMany(x => x.CustomTables).HasForeignKey(x => x.CreatedBy).OnDelete(DeleteBehavior.Restrict);
        });

        // ── custom_columns ───────────────────────────────────────
        mb.Entity<CustomColumn>(e =>
        {
            e.ToTable("custom_columns");
            e.HasKey(x => x.ColumnId);
            e.Property(x => x.ColumnId).HasColumnName("column_id");
            e.Property(x => x.TableId).HasColumnName("table_id");
            e.Property(x => x.ColumnName).HasColumnName("column_name").HasMaxLength(100);
            e.Property(x => x.DataType).HasColumnName("data_type").HasConversion<string>();
            e.Property(x => x.DisplayOrder).HasColumnName("display_order");
            e.Property(x => x.IsRequired).HasColumnName("is_required");
            e.Property(x => x.DropdownOptions).HasColumnName("dropdown_options").HasColumnType("json");
            e.Property(x => x.IsDeleted).HasColumnName("is_deleted");

            e.HasOne(x => x.Table).WithMany(x => x.Columns).HasForeignKey(x => x.TableId).OnDelete(DeleteBehavior.Cascade);
        });

        // ── custom_rows ──────────────────────────────────────────
        mb.Entity<CustomRow>(e =>
        {
            e.ToTable("custom_rows");
            e.HasKey(x => x.RowId);
            e.Property(x => x.RowId).HasColumnName("row_id");
            e.Property(x => x.TableId).HasColumnName("table_id");
            e.Property(x => x.CreatedBy).HasColumnName("created_by");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            e.Property(x => x.IsDeleted).HasColumnName("is_deleted");

            e.HasOne(x => x.Table).WithMany(x => x.Rows).HasForeignKey(x => x.TableId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.CreatedByEngineer).WithMany(x => x.CustomRows).HasForeignKey(x => x.CreatedBy).OnDelete(DeleteBehavior.Restrict);
        });

        // ── custom_cells ─────────────────────────────────────────
        mb.Entity<CustomCell>(e =>
        {
            e.ToTable("custom_cells");
            e.HasKey(x => x.CellId);
            e.Property(x => x.CellId).HasColumnName("cell_id");
            e.Property(x => x.RowId).HasColumnName("row_id");
            e.Property(x => x.ColumnId).HasColumnName("column_id");
            e.Property(x => x.Value).HasColumnName("value");
            e.HasIndex(x => new { x.RowId, x.ColumnId }).IsUnique();

            e.HasOne(x => x.Row).WithMany(x => x.Cells).HasForeignKey(x => x.RowId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Column).WithMany(x => x.Cells).HasForeignKey(x => x.ColumnId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
