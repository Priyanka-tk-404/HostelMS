-- =============================================================
--  HostelMS - MySQL Complete Database Setup Script
--  Run this file as MySQL root:
--  mysql -u root -p < mysql-setup.sql
-- =============================================================

-- 1. Create Database
DROP DATABASE IF EXISTS hostelms_db;
CREATE DATABASE hostelms_db
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;

USE hostelms_db;

-- 2. Create dedicated user
DROP USER IF EXISTS 'hostelms_user'@'localhost';
CREATE USER 'hostelms_user'@'localhost' IDENTIFIED BY 'HostelMS@2024';
GRANT ALL PRIVILEGES ON hostelms_db.* TO 'hostelms_user'@'localhost';
FLUSH PRIVILEGES;

SELECT 'Database and user created successfully!' AS Status;

-- =============================================================
-- NOTE: Tables are created automatically by EF Core migrations.
-- After running this script, run:  dotnet ef database update
-- (in the HostelMS.API folder)
-- =============================================================

-- Verify
SHOW DATABASES LIKE 'hostelms_db';
SELECT User, Host FROM mysql.user WHERE User = 'hostelms_user';
