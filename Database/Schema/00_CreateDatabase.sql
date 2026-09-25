-- Personal Finance Tracker
-- Creates the database if it does not already exist.

IF NOT EXISTS
(
    SELECT *
    FROM sys.databases
    WHERE name = 'PersonalFinanceTracker'
)
BEGIN
    CREATE DATABASE PersonalFinanceTracker;
END
GO