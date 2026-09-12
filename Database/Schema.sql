-- Personal Finance Tracker - Database Schema
-- Run against SQL Server or LocalDB
--
-- This script drops and recreates every table below. That's fine for a
-- fresh/dev database - do NOT re-run it against a database that already has
-- real accounts/transactions in it, since DROP TABLE deletes them irrecoverably.

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'PersonalFinanceTracker')
BEGIN
    CREATE DATABASE PersonalFinanceTracker;
END
GO

USE PersonalFinanceTracker;
GO

IF OBJECT_ID('dbo.Transactions', 'U') IS NOT NULL DROP TABLE dbo.Transactions;
IF OBJECT_ID('dbo.Categories', 'U') IS NOT NULL DROP TABLE dbo.Categories;
IF OBJECT_ID('dbo.Accounts', 'U') IS NOT NULL DROP TABLE dbo.Accounts;
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE dbo.Users;
GO

-- ============================================================================
-- Users - Admin manages the system; User owns their own financial data.
-- An Admin account never has accounts/categories/transactions of its own.
-- ============================================================================
CREATE TABLE dbo.Users
(
    Id           INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Name         NVARCHAR(100)     NOT NULL,
    Email        NVARCHAR(256)     NOT NULL,
    PasswordHash NVARCHAR(MAX)     NOT NULL,
    Role         NVARCHAR(20)      NOT NULL CONSTRAINT DF_Users_Role DEFAULT ('User'),
    IsActive     BIT               NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT (1),
    CreatedAt    DATETIME2         NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT UQ_Users_Email UNIQUE (Email),
    CONSTRAINT CK_Users_Role CHECK (Role IN ('Admin', 'User')),
    CONSTRAINT CK_Users_Name_NotEmpty CHECK (LEN(LTRIM(RTRIM(Name))) > 0),
    CONSTRAINT CK_Users_Email_NotEmpty CHECK (LEN(LTRIM(RTRIM(Email))) > 0)
);
GO

CREATE TABLE dbo.Accounts
(
    Id           INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    UserId       INT               NOT NULL,
    Name         NVARCHAR(100)     NOT NULL,
    AccountType  NVARCHAR(50)      NOT NULL,
    InitialBalance DECIMAL(18,2)     NOT NULL DEFAULT (0),
    Balance      DECIMAL(18,2)     NOT NULL CONSTRAINT DF_Accounts_Balance DEFAULT (0),
    CreatedAt    DATETIME2         NOT NULL CONSTRAINT DF_Accounts_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT FK_Accounts_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id)
);
GO

CREATE TABLE dbo.Categories
(
    Id     INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    UserId INT               NOT NULL,
    Name   NVARCHAR(100)     NOT NULL,
    Type   NVARCHAR(20)      NOT NULL CHECK (Type IN ('Income', 'Expense')),
    Color  NVARCHAR(7)       NOT NULL CONSTRAINT DF_Categories_Color DEFAULT ('#6366f1'),
    CONSTRAINT FK_Categories_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id)
);
GO

CREATE TABLE dbo.Transactions
(
    Id              INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    UserId          INT               NOT NULL,
    AccountId       INT               NOT NULL,
    CategoryId      INT               NOT NULL,
    Amount          DECIMAL(18,2)     NOT NULL CHECK (Amount > 0),
    Description     NVARCHAR(500)     NULL,
    TransactionDate DATE              NOT NULL,
    TransactionType NVARCHAR(20)      NOT NULL CHECK (TransactionType IN ('Income', 'Expense')),
    CreatedAt       DATETIME2         NOT NULL CONSTRAINT DF_Transactions_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT FK_Transactions_Users      FOREIGN KEY (UserId)     REFERENCES dbo.Users(Id),
    CONSTRAINT FK_Transactions_Accounts   FOREIGN KEY (AccountId)  REFERENCES dbo.Accounts(Id),
    CONSTRAINT FK_Transactions_Categories FOREIGN KEY (CategoryId) REFERENCES dbo.Categories(Id)
);
GO

CREATE INDEX IX_Accounts_UserId ON dbo.Accounts(UserId);
CREATE INDEX IX_Categories_UserId ON dbo.Categories(UserId);
CREATE INDEX IX_Transactions_UserId_Date ON dbo.Transactions(UserId, TransactionDate DESC);
CREATE INDEX IX_Transactions_AccountId ON dbo.Transactions(AccountId);
CREATE INDEX IX_Transactions_CategoryId ON dbo.Transactions(CategoryId);
GO
