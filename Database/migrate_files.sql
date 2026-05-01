-- ─────────────────────────────────────────────────────────────
-- Migration: drop attachments + drive_folders + google_oauth_token
--            create new `files` table
-- Run on both DEV and PROD before deploying.
-- ─────────────────────────────────────────────────────────────

SET FOREIGN_KEY_CHECKS = 0;

DROP TABLE IF EXISTS attachments;
DROP TABLE IF EXISTS attachment_categories;
DROP TABLE IF EXISTS attachment_types;
DROP TABLE IF EXISTS drive_folders;
DROP TABLE IF EXISTS google_oauth_token;

SET FOREIGN_KEY_CHECKS = 1;

CREATE TABLE IF NOT EXISTS files (
    id          INT AUTO_INCREMENT PRIMARY KEY,
    work_id     INT          NOT NULL,
    file_type   VARCHAR(10)  NOT NULL COMMENT 'pdf or image',
    file_url    VARCHAR(1000) NOT NULL,
    uploaded_by INT          NULL,
    created_at  DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_files_work (work_id),
    CONSTRAINT fk_files_work     FOREIGN KEY (work_id)     REFERENCES works(work_id)         ON DELETE CASCADE,
    CONSTRAINT fk_files_engineer FOREIGN KEY (uploaded_by) REFERENCES engineers(engineer_id) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
