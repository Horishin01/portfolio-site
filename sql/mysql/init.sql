-- This script is for MySQL / MariaDB.
CREATE DATABASE IF NOT EXISTS `portfolio_site`
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;

USE `portfolio_site`;

CREATE TABLE IF NOT EXISTS `portfolio_documents` (
  `id` TINYINT UNSIGNED NOT NULL PRIMARY KEY,
  `data` LONGTEXT NOT NULL,
  `updated_at` VARCHAR(32) NOT NULL
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
