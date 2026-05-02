-- Running bill / payment history per work
CREATE TABLE IF NOT EXISTS work_bills (
    bill_id            INT AUTO_INCREMENT PRIMARY KEY,
    work_id            INT NOT NULL,
    bill_number        VARCHAR(100) NOT NULL,
    bill_date          DATE NULL,
    gross_amount       DECIMAL(12,4) NOT NULL DEFAULT 0,
    deductions         DECIMAL(12,4) NOT NULL DEFAULT 0,
    net_amount         DECIMAL(12,4) NOT NULL DEFAULT 0,
    cumulative_billed  DECIMAL(12,4) NOT NULL DEFAULT 0,
    status             ENUM('SUBMITTED','PASSED','PAID','REJECTED') NOT NULL DEFAULT 'SUBMITTED',
    remarks            TEXT NULL,
    pdf_url            VARCHAR(1000) NULL,
    created_by         INT NULL,
    created_at         DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at         DATETIME NULL,
    FOREIGN KEY (work_id)   REFERENCES works(work_id)          ON DELETE CASCADE,
    FOREIGN KEY (created_by) REFERENCES engineers(engineer_id) ON DELETE SET NULL
);
