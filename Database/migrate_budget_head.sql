-- Create Budget Head Master table
CREATE TABLE IF NOT EXISTS budget_head_master (
    budget_head_id   INT AUTO_INCREMENT PRIMARY KEY,
    budget_code      VARCHAR(50)     NOT NULL,
    budget_name      VARCHAR(200)    NOT NULL,
    available_fund   DECIMAL(14,4)   NOT NULL DEFAULT 0,
    amount_spent     DECIMAL(14,4)   NOT NULL DEFAULT 0,
    remaining_amount DECIMAL(14,4)   NOT NULL DEFAULT 0,
    updated_by       VARCHAR(200)    NULL,
    updated_at       DATETIME        NULL,
    is_active        TINYINT(1)      NOT NULL DEFAULT 1,
    created_at       DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE KEY uk_budget_code (budget_code)
);
