-- ============================================================
-- BDA Work Dashboard — Production DB bootstrap
-- Run once on VPS:
--   mysql -u root -p < /tmp/setup_prod_db.sql
-- ============================================================

CREATE DATABASE IF NOT EXISTS bda_work_dashboard_prod
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;

-- Create app user (change password before running!)
CREATE USER IF NOT EXISTS 'bdaworksuser'@'localhost'
  IDENTIFIED BY 'CHANGE_THIS_PASSWORD';

GRANT ALL PRIVILEGES ON bda_work_dashboard_prod.*
  TO 'bdaworksuser'@'localhost';

FLUSH PRIVILEGES;

-- Tables are created by EF Core on first run (EnsureCreated / migrate).
-- This file only provisions the DB and user.
