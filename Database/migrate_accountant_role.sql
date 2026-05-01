-- Add ACCOUNTANT to the engineers role enum
ALTER TABLE engineers
    MODIFY COLUMN role ENUM('ADMIN','JEN','AEN','XEN','ACCOUNTANT') NOT NULL DEFAULT 'JEN';
