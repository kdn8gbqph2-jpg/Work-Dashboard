-- Add budget_head_id column to work_bills table
ALTER TABLE work_bills
    ADD COLUMN budget_head_id INT NULL AFTER pdf_url,
    ADD CONSTRAINT fk_work_bills_budget_head
        FOREIGN KEY (budget_head_id) REFERENCES budget_head_master(budget_head_id)
        ON DELETE SET NULL;
