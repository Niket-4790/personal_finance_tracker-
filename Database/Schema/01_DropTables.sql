USE PersonalFinanceTracker;
GO

-- Drop dependent tables first.

IF OBJECT_ID('dbo.Payments', 'U') IS NOT NULL
    DROP TABLE dbo.Payments;

IF OBJECT_ID('dbo.Budgets', 'U') IS NOT NULL
    DROP TABLE dbo.Budgets;

IF OBJECT_ID('dbo.Transactions', 'U') IS NOT NULL
    DROP TABLE dbo.Transactions;

IF OBJECT_ID('dbo.Categories', 'U') IS NOT NULL
    DROP TABLE dbo.Categories;

IF OBJECT_ID('dbo.Accounts', 'U') IS NOT NULL
    DROP TABLE dbo.Accounts;

IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL
    DROP TABLE dbo.Users;
GO