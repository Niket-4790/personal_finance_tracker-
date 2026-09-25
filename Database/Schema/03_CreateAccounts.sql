USE PersonalFinanceTracker;
GO

CREATE TABLE dbo.Accounts
(
    Id INT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_Accounts PRIMARY KEY,

    UserId INT NOT NULL,

    Name NVARCHAR(100) NOT NULL,

    AccountType NVARCHAR(50) NOT NULL,

    InitialBalance DECIMAL(18,2) NOT NULL
        CONSTRAINT DF_Accounts_InitialBalance
        DEFAULT (0),

    Balance DECIMAL(18,2) NOT NULL
        CONSTRAINT DF_Accounts_Balance
        DEFAULT (0),

    CreatedAt DATETIME2 NOT NULL
        CONSTRAINT DF_Accounts_CreatedAt
        DEFAULT (SYSUTCDATETIME()),

    CONSTRAINT FK_Accounts_Users
        FOREIGN KEY (UserId)
        REFERENCES dbo.Users(Id)
);
GO