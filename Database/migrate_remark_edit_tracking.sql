-- Migration: add edit-tracking columns to work_remarks
-- Run on both DEV and PROD before deploying.

ALTER TABLE work_remarks
    ADD COLUMN edited_by_name VARCHAR(200) NULL AFTER created_at,
    ADD COLUMN edited_by_id   INT          NULL AFTER edited_by_name,
    ADD COLUMN edited_at      DATETIME     NULL AFTER edited_by_id;
