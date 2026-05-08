using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkDashboard.Migrations
{
    /// <inheritdoc />
    public partial class AddAnfAndWorkOrderFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "budget_head_master",
                columns: table => new
                {
                    budget_head_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    budget_code = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    budget_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    available_fund = table.Column<decimal>(type: "decimal(14,4)", precision: 14, scale: 4, nullable: false),
                    amount_spent = table.Column<decimal>(type: "decimal(14,4)", precision: 14, scale: 4, nullable: false),
                    remaining_amount = table.Column<decimal>(type: "decimal(14,4)", precision: 14, scale: 4, nullable: false),
                    updated_by = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_budget_head_master", x => x.budget_head_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "engineers",
                columns: table => new
                {
                    engineer_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    username = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    password_hash = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    role = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    mobile = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_engineers", x => x.engineer_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "fund_sources",
                columns: table => new
                {
                    fund_source_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    source_code = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    source_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fund_sources", x => x.fund_source_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "work_categories",
                columns: table => new
                {
                    category_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    category_code = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    category_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_work_categories", x => x.category_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "audit_log",
                columns: table => new
                {
                    log_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    table_name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    record_id = table.Column<int>(type: "int", nullable: false),
                    action = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    changed_by = table.Column<int>(type: "int", nullable: true),
                    changed_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    details = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_log", x => x.log_id);
                    table.ForeignKey(
                        name: "FK_audit_log_engineers_changed_by",
                        column: x => x.changed_by,
                        principalTable: "engineers",
                        principalColumn: "engineer_id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "works",
                columns: table => new
                {
                    work_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    work_name = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    work_code = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    category_id = table.Column<int>(type: "int", nullable: true),
                    fund_source_id = table.Column<int>(type: "int", nullable: true),
                    department = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    assigned_jen_id = table.Column<int>(type: "int", nullable: true),
                    assigned_aen_id = table.Column<int>(type: "int", nullable: true),
                    assigned_exen_id = table.Column<int>(type: "int", nullable: true),
                    contractor_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    contractor_mobile = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    sanctioned_amount = table.Column<decimal>(type: "decimal(12,4)", precision: 12, scale: 4, nullable: true),
                    AnfNo = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AnfDate = table.Column<DateOnly>(type: "date", nullable: true),
                    agreement_amount = table.Column<decimal>(type: "decimal(12,4)", precision: 12, scale: 4, nullable: true),
                    WorkOrderNo = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    WorkOrderDate = table.Column<DateOnly>(type: "date", nullable: true),
                    expenditure = table.Column<decimal>(type: "decimal(12,4)", precision: 12, scale: 4, nullable: true),
                    progress_percent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    financial_progress_percent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    payment_status = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_annual_contract = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    is_scheme = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    is_cm_budget = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: true),
                    expected_completion = table.Column<DateOnly>(type: "date", nullable: true),
                    actual_completion = table.Column<DateOnly>(type: "date", nullable: true),
                    location = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ward_number = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    remarks = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_works", x => x.work_id);
                    table.ForeignKey(
                        name: "FK_works_engineers_assigned_aen_id",
                        column: x => x.assigned_aen_id,
                        principalTable: "engineers",
                        principalColumn: "engineer_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_works_engineers_assigned_exen_id",
                        column: x => x.assigned_exen_id,
                        principalTable: "engineers",
                        principalColumn: "engineer_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_works_engineers_assigned_jen_id",
                        column: x => x.assigned_jen_id,
                        principalTable: "engineers",
                        principalColumn: "engineer_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_works_engineers_created_by",
                        column: x => x.created_by,
                        principalTable: "engineers",
                        principalColumn: "engineer_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_works_fund_sources_fund_source_id",
                        column: x => x.fund_source_id,
                        principalTable: "fund_sources",
                        principalColumn: "fund_source_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_works_work_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "work_categories",
                        principalColumn: "category_id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "files",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    work_id = table.Column<int>(type: "int", nullable: false),
                    file_type = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    file_url = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    uploaded_by = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_files", x => x.id);
                    table.ForeignKey(
                        name: "FK_files_engineers_uploaded_by",
                        column: x => x.uploaded_by,
                        principalTable: "engineers",
                        principalColumn: "engineer_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_files_works_work_id",
                        column: x => x.work_id,
                        principalTable: "works",
                        principalColumn: "work_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "work_bills",
                columns: table => new
                {
                    bill_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    work_id = table.Column<int>(type: "int", nullable: false),
                    bill_number = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    bill_date = table.Column<DateOnly>(type: "date", nullable: true),
                    gross_amount = table.Column<decimal>(type: "decimal(12,4)", precision: 12, scale: 4, nullable: false),
                    deductions = table.Column<decimal>(type: "decimal(12,4)", precision: 12, scale: 4, nullable: false),
                    net_amount = table.Column<decimal>(type: "decimal(12,4)", precision: 12, scale: 4, nullable: false),
                    cumulative_billed = table.Column<decimal>(type: "decimal(12,4)", precision: 12, scale: 4, nullable: false),
                    status = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    remarks = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    pdf_url = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    budget_head_id = table.Column<int>(type: "int", nullable: true),
                    created_by = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_work_bills", x => x.bill_id);
                    table.ForeignKey(
                        name: "FK_work_bills_budget_head_master_budget_head_id",
                        column: x => x.budget_head_id,
                        principalTable: "budget_head_master",
                        principalColumn: "budget_head_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_work_bills_engineers_created_by",
                        column: x => x.created_by,
                        principalTable: "engineers",
                        principalColumn: "engineer_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_work_bills_works_work_id",
                        column: x => x.work_id,
                        principalTable: "works",
                        principalColumn: "work_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "work_progress_log",
                columns: table => new
                {
                    log_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    work_id = table.Column<int>(type: "int", nullable: false),
                    progress_percent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    remarks = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    logged_by = table.Column<int>(type: "int", nullable: true),
                    logged_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_work_progress_log", x => x.log_id);
                    table.ForeignKey(
                        name: "FK_work_progress_log_engineers_logged_by",
                        column: x => x.logged_by,
                        principalTable: "engineers",
                        principalColumn: "engineer_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_work_progress_log_works_work_id",
                        column: x => x.work_id,
                        principalTable: "works",
                        principalColumn: "work_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "work_remarks",
                columns: table => new
                {
                    remark_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    work_id = table.Column<int>(type: "int", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    author_name = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    author_id = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    edited_by_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    edited_by_id = table.Column<int>(type: "int", nullable: true),
                    edited_at = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_work_remarks", x => x.remark_id);
                    table.ForeignKey(
                        name: "FK_work_remarks_engineers_author_id",
                        column: x => x.author_id,
                        principalTable: "engineers",
                        principalColumn: "engineer_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_work_remarks_works_work_id",
                        column: x => x.work_id,
                        principalTable: "works",
                        principalColumn: "work_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_audit_log_changed_by",
                table: "audit_log",
                column: "changed_by");

            migrationBuilder.CreateIndex(
                name: "IX_budget_head_master_budget_code",
                table: "budget_head_master",
                column: "budget_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_engineers_username",
                table: "engineers",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_files_uploaded_by",
                table: "files",
                column: "uploaded_by");

            migrationBuilder.CreateIndex(
                name: "IX_files_work_id",
                table: "files",
                column: "work_id");

            migrationBuilder.CreateIndex(
                name: "IX_fund_sources_source_code",
                table: "fund_sources",
                column: "source_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_work_bills_budget_head_id",
                table: "work_bills",
                column: "budget_head_id");

            migrationBuilder.CreateIndex(
                name: "IX_work_bills_created_by",
                table: "work_bills",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_work_bills_work_id",
                table: "work_bills",
                column: "work_id");

            migrationBuilder.CreateIndex(
                name: "IX_work_categories_category_code",
                table: "work_categories",
                column: "category_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_work_progress_log_logged_by",
                table: "work_progress_log",
                column: "logged_by");

            migrationBuilder.CreateIndex(
                name: "IX_work_progress_log_work_id",
                table: "work_progress_log",
                column: "work_id");

            migrationBuilder.CreateIndex(
                name: "IX_work_remarks_author_id",
                table: "work_remarks",
                column: "author_id");

            migrationBuilder.CreateIndex(
                name: "IX_work_remarks_work_id",
                table: "work_remarks",
                column: "work_id");

            migrationBuilder.CreateIndex(
                name: "IX_works_assigned_aen_id",
                table: "works",
                column: "assigned_aen_id");

            migrationBuilder.CreateIndex(
                name: "IX_works_assigned_exen_id",
                table: "works",
                column: "assigned_exen_id");

            migrationBuilder.CreateIndex(
                name: "IX_works_assigned_jen_id",
                table: "works",
                column: "assigned_jen_id");

            migrationBuilder.CreateIndex(
                name: "IX_works_category_id",
                table: "works",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_works_created_by",
                table: "works",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_works_fund_source_id",
                table: "works",
                column: "fund_source_id");

            migrationBuilder.CreateIndex(
                name: "IX_works_work_code",
                table: "works",
                column: "work_code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_log");

            migrationBuilder.DropTable(
                name: "files");

            migrationBuilder.DropTable(
                name: "work_bills");

            migrationBuilder.DropTable(
                name: "work_progress_log");

            migrationBuilder.DropTable(
                name: "work_remarks");

            migrationBuilder.DropTable(
                name: "budget_head_master");

            migrationBuilder.DropTable(
                name: "works");

            migrationBuilder.DropTable(
                name: "engineers");

            migrationBuilder.DropTable(
                name: "fund_sources");

            migrationBuilder.DropTable(
                name: "work_categories");
        }
    }
}
