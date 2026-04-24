-- BDA Work Dashboard Database Schema
-- Database: bda_work_dashboard_dev
-- Charset: utf8mb4_unicode_ci (Hindi/Devanagari support)

USE bda_work_dashboard_dev;

-- ─────────────────────────────────────────
-- 1. engineers
-- ─────────────────────────────────────────
CREATE TABLE IF NOT EXISTS engineers (
    engineer_id   INT            NOT NULL AUTO_INCREMENT,
    name          VARCHAR(150)   NOT NULL,
    username      VARCHAR(50)    NOT NULL,
    password_hash VARCHAR(255)   NOT NULL,
    role          ENUM('ADMIN','JEN') NOT NULL DEFAULT 'JEN',
    mobile        VARCHAR(15)    NULL,
    email         VARCHAR(100)   NULL,
    is_active     TINYINT(1)     NOT NULL DEFAULT 1,
    is_deleted    TINYINT(1)     NOT NULL DEFAULT 0,
    created_at    DATETIME       NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at    DATETIME       NULL ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (engineer_id),
    UNIQUE KEY uq_engineers_username (username)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ─────────────────────────────────────────
-- 2. work_categories
-- ─────────────────────────────────────────
CREATE TABLE IF NOT EXISTS work_categories (
    category_id   INT            NOT NULL AUTO_INCREMENT,
    category_code VARCHAR(20)    NOT NULL,
    category_name VARCHAR(100)   NOT NULL,
    is_active     TINYINT(1)     NOT NULL DEFAULT 1,
    PRIMARY KEY (category_id),
    UNIQUE KEY uq_work_categories_code (category_code)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO work_categories (category_code, category_name) VALUES
    ('ROAD',      'Road'),
    ('DRAIN',     'Drain / Nala'),
    ('BUILDING',  'Building'),
    ('ELECTRIC',  'Electrical / Lighting'),
    ('PARK',      'Park / Greenery'),
    ('MISC',      'Miscellaneous'),
    ('DEVISTHAN', 'Devisthan');

-- ─────────────────────────────────────────
-- 3. fund_sources
-- ─────────────────────────────────────────
CREATE TABLE IF NOT EXISTS fund_sources (
    fund_source_id INT           NOT NULL AUTO_INCREMENT,
    source_code    VARCHAR(20)   NOT NULL,
    source_name    VARCHAR(100)  NOT NULL,
    is_active      TINYINT(1)    NOT NULL DEFAULT 1,
    PRIMARY KEY (fund_source_id),
    UNIQUE KEY uq_fund_sources_code (source_code)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO fund_sources (source_code, source_name) VALUES
    ('BDA',    'BDA'),
    ('LOAN',   'Loan'),
    ('DEPOSIT','Deposit'),
    ('GRANT',  'Grant'),
    ('NCRPB',  'NCRPB'),
    ('SASCI',  'SASCI');

-- ─────────────────────────────────────────
-- 4. works
-- ─────────────────────────────────────────
CREATE TABLE IF NOT EXISTS works (
    work_id                    INT             NOT NULL AUTO_INCREMENT,
    work_name                  VARCHAR(1000)   NOT NULL,
    work_code                  VARCHAR(50)     NULL,
    category_id                INT             NULL,
    fund_source_id             INT             NULL,
    department                 VARCHAR(50)     NULL,
    assigned_jen_id            INT             NULL,
    assigned_aen_id            INT             NULL,
    assigned_exen_id           INT             NULL,
    contractor_name            VARCHAR(200)    NULL,
    contractor_mobile          VARCHAR(15)     NULL,
    sanctioned_amount          DECIMAL(12,4)   NULL COMMENT 'in Lakhs',
    agreement_amount           DECIMAL(12,4)   NULL COMMENT 'in Lakhs',
    expenditure                DECIMAL(12,4)   NULL COMMENT 'in Lakhs',
    progress_percent           DECIMAL(5,2)    NULL,
    financial_progress_percent DECIMAL(5,2)    NULL,
    payment_status             VARCHAR(100)    NULL,
    status                     ENUM('ONGOING','COMPLETED','STALLED','CANCELLED') NOT NULL DEFAULT 'ONGOING',
    is_annual_contract         TINYINT(1)      NOT NULL DEFAULT 0,
    is_scheme                  TINYINT(1)      NOT NULL DEFAULT 0 COMMENT 'S=1 NS=0',
    is_cm_budget               TINYINT(1)      NOT NULL DEFAULT 0,
    start_date                 DATE            NULL,
    expected_completion        DATE            NULL,
    actual_completion          DATE            NULL,
    location                   VARCHAR(200)    NULL,
    ward_number                VARCHAR(20)     NULL,
    remarks                    TEXT            NULL,
    is_deleted                 TINYINT(1)      NOT NULL DEFAULT 0,
    created_by                 INT             NULL,
    created_at                 DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at                 DATETIME        NULL ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (work_id),
    UNIQUE KEY uq_works_code (work_code),
    KEY idx_works_status        (status),
    KEY idx_works_annual        (is_annual_contract),
    KEY idx_works_jen           (assigned_jen_id),
    KEY idx_works_category      (category_id),
    KEY idx_works_fund_source   (fund_source_id),
    CONSTRAINT fk_works_category    FOREIGN KEY (category_id)     REFERENCES work_categories (category_id),
    CONSTRAINT fk_works_fund_source FOREIGN KEY (fund_source_id)  REFERENCES fund_sources    (fund_source_id),
    CONSTRAINT fk_works_jen         FOREIGN KEY (assigned_jen_id) REFERENCES engineers       (engineer_id),
    CONSTRAINT fk_works_aen         FOREIGN KEY (assigned_aen_id) REFERENCES engineers       (engineer_id),
    CONSTRAINT fk_works_exen        FOREIGN KEY (assigned_exen_id)REFERENCES engineers       (engineer_id),
    CONSTRAINT fk_works_created_by  FOREIGN KEY (created_by)      REFERENCES engineers       (engineer_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ─────────────────────────────────────────
-- 5. work_progress_log
-- ─────────────────────────────────────────
CREATE TABLE IF NOT EXISTS work_progress_log (
    log_id           INT          NOT NULL AUTO_INCREMENT,
    work_id          INT          NOT NULL,
    progress_percent DECIMAL(5,2) NULL,
    remarks          TEXT         NULL,
    logged_by        INT          NULL,
    logged_at        DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (log_id),
    KEY idx_progress_log_work (work_id),
    CONSTRAINT fk_progress_log_work      FOREIGN KEY (work_id)   REFERENCES works     (work_id),
    CONSTRAINT fk_progress_log_logged_by FOREIGN KEY (logged_by) REFERENCES engineers (engineer_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ─────────────────────────────────────────
-- 6. attachment_types
-- ─────────────────────────────────────────
CREATE TABLE IF NOT EXISTS attachment_types (
    type_id   INT          NOT NULL AUTO_INCREMENT,
    type_code VARCHAR(20)  NOT NULL,
    type_name VARCHAR(50)  NOT NULL,
    PRIMARY KEY (type_id),
    UNIQUE KEY uq_attachment_types_code (type_code)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO attachment_types (type_code, type_name) VALUES
    ('PDF',     'PDF Document'),
    ('IMAGE',   'Image / Photo'),
    ('VIDEO',   'Video'),
    ('DRAWING', 'Drawing / DPR'),
    ('OTHER',   'Other');

-- ─────────────────────────────────────────
-- 7. attachment_categories
-- ─────────────────────────────────────────
CREATE TABLE IF NOT EXISTS attachment_categories (
    cat_id          INT          NOT NULL AUTO_INCREMENT,
    cat_code        VARCHAR(30)  NOT NULL,
    cat_name        VARCHAR(100) NOT NULL,
    allowed_type_id INT          NULL,
    PRIMARY KEY (cat_id),
    UNIQUE KEY uq_attachment_categories_code (cat_code),
    CONSTRAINT fk_att_cat_type FOREIGN KEY (allowed_type_id) REFERENCES attachment_types (type_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO attachment_categories (cat_code, cat_name, allowed_type_id) VALUES
    ('BEFORE_PHOTO',    'Before Work Photo',     2),
    ('PROGRESS_PHOTO',  'Progress Photo',        2),
    ('AFTER_PHOTO',     'After / Completion Photo', 2),
    ('ESTIMATE',        'Estimate / DPR',        1),
    ('WORK_ORDER',      'Work Order',            1),
    ('RA_BILL',         'R.A. Bill',             1),
    ('MB',              'Measurement Book',      1),
    ('OTHER_DOC',       'Other Document',        1);

-- ─────────────────────────────────────────
-- 8. attachments  (polymorphic)
-- ─────────────────────────────────────────
CREATE TABLE IF NOT EXISTS attachments (
    attachment_id   INT           NOT NULL AUTO_INCREMENT,
    parent_type     ENUM('WORK','PROGRESS_LOG','CUSTOM_ROW') NOT NULL,
    parent_id       INT           NOT NULL,
    type_id         INT           NULL,
    cat_id          INT           NULL,
    file_name       VARCHAR(255)  NOT NULL,
    drive_file_id   VARCHAR(150)  NULL,
    drive_view_url  VARCHAR(1000) NULL,
    file_size_bytes BIGINT        NULL,
    mime_type       VARCHAR(100)  NULL,
    uploaded_by     INT           NULL,
    uploaded_at     DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,
    is_deleted      TINYINT(1)    NOT NULL DEFAULT 0,
    PRIMARY KEY (attachment_id),
    KEY idx_attachments_parent (parent_type, parent_id),
    CONSTRAINT fk_attachments_type        FOREIGN KEY (type_id)     REFERENCES attachment_types      (type_id),
    CONSTRAINT fk_attachments_cat         FOREIGN KEY (cat_id)      REFERENCES attachment_categories (cat_id),
    CONSTRAINT fk_attachments_uploaded_by FOREIGN KEY (uploaded_by) REFERENCES engineers             (engineer_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ─────────────────────────────────────────
-- 9. drive_folders
-- ─────────────────────────────────────────
CREATE TABLE IF NOT EXISTS drive_folders (
    folder_id          INT          NOT NULL AUTO_INCREMENT,
    work_id            INT          NOT NULL,
    root_folder_id     VARCHAR(150) NULL,
    photos_folder_id   VARCHAR(150) NULL,
    pdfs_folder_id     VARCHAR(150) NULL,
    videos_folder_id   VARCHAR(150) NULL,
    drawings_folder_id VARCHAR(150) NULL,
    created_at         DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (folder_id),
    UNIQUE KEY uq_drive_folders_work (work_id),
    CONSTRAINT fk_drive_folders_work FOREIGN KEY (work_id) REFERENCES works (work_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ─────────────────────────────────────────
-- 10. google_oauth_token  (singleton)
-- ─────────────────────────────────────────
CREATE TABLE IF NOT EXISTS google_oauth_token (
    token_id      INT          NOT NULL,
    client_id     VARCHAR(200) NULL,
    client_secret VARCHAR(200) NULL,
    refresh_token TEXT         NULL,
    access_token  TEXT         NULL,
    token_expiry  DATETIME     NULL,
    updated_at    DATETIME     NULL ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (token_id),
    CONSTRAINT chk_singleton CHECK (token_id = 1)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ─────────────────────────────────────────
-- 11. audit_log
-- ─────────────────────────────────────────
CREATE TABLE IF NOT EXISTS audit_log (
    log_id     BIGINT       NOT NULL AUTO_INCREMENT,
    table_name VARCHAR(50)  NOT NULL,
    record_id  INT          NOT NULL,
    action     ENUM('INSERT','UPDATE','DELETE') NOT NULL,
    changed_by INT          NULL,
    changed_at DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    details    JSON         NULL,
    PRIMARY KEY (log_id),
    KEY idx_audit_log_table  (table_name, record_id),
    KEY idx_audit_log_by     (changed_by),
    CONSTRAINT fk_audit_log_changed_by FOREIGN KEY (changed_by) REFERENCES engineers (engineer_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ─────────────────────────────────────────
-- 12. custom_tables
-- ─────────────────────────────────────────
CREATE TABLE IF NOT EXISTS custom_tables (
    table_id    INT           NOT NULL AUTO_INCREMENT,
    table_name  VARCHAR(200)  NOT NULL,
    description TEXT          NULL,
    work_id     INT           NULL COMMENT 'NULL = standalone register',
    created_by  INT           NULL,
    is_deleted  TINYINT(1)    NOT NULL DEFAULT 0,
    created_at  DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at  DATETIME      NULL ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (table_id),
    KEY idx_custom_tables_work (work_id),
    CONSTRAINT fk_custom_tables_work       FOREIGN KEY (work_id)    REFERENCES works     (work_id),
    CONSTRAINT fk_custom_tables_created_by FOREIGN KEY (created_by) REFERENCES engineers (engineer_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ─────────────────────────────────────────
-- 13. custom_columns
-- ─────────────────────────────────────────
CREATE TABLE IF NOT EXISTS custom_columns (
    column_id        INT          NOT NULL AUTO_INCREMENT,
    table_id         INT          NOT NULL,
    column_name      VARCHAR(100) NOT NULL,
    data_type        ENUM('TEXT','NUMBER','DATE','BOOLEAN','DROPDOWN') NOT NULL DEFAULT 'TEXT',
    display_order    INT          NOT NULL DEFAULT 0,
    is_required      TINYINT(1)   NOT NULL DEFAULT 0,
    dropdown_options JSON         NULL,
    is_deleted       TINYINT(1)   NOT NULL DEFAULT 0,
    PRIMARY KEY (column_id),
    KEY idx_custom_columns_table (table_id),
    CONSTRAINT fk_custom_columns_table FOREIGN KEY (table_id) REFERENCES custom_tables (table_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ─────────────────────────────────────────
-- 14. custom_rows
-- ─────────────────────────────────────────
CREATE TABLE IF NOT EXISTS custom_rows (
    row_id     INT       NOT NULL AUTO_INCREMENT,
    table_id   INT       NOT NULL,
    created_by INT       NULL,
    created_at DATETIME  NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME  NULL ON UPDATE CURRENT_TIMESTAMP,
    is_deleted TINYINT(1) NOT NULL DEFAULT 0,
    PRIMARY KEY (row_id),
    KEY idx_custom_rows_table (table_id),
    CONSTRAINT fk_custom_rows_table      FOREIGN KEY (table_id)   REFERENCES custom_tables (table_id),
    CONSTRAINT fk_custom_rows_created_by FOREIGN KEY (created_by) REFERENCES engineers     (engineer_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ─────────────────────────────────────────
-- 15. custom_cells
-- ─────────────────────────────────────────
CREATE TABLE IF NOT EXISTS custom_cells (
    cell_id   BIGINT       NOT NULL AUTO_INCREMENT,
    row_id    INT          NOT NULL,
    column_id INT          NOT NULL,
    value     TEXT         NULL,
    PRIMARY KEY (cell_id),
    UNIQUE KEY uq_custom_cells (row_id, column_id),
    KEY idx_custom_cells_column (column_id),
    CONSTRAINT fk_custom_cells_row    FOREIGN KEY (row_id)    REFERENCES custom_rows    (row_id),
    CONSTRAINT fk_custom_cells_column FOREIGN KEY (column_id) REFERENCES custom_columns (column_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
