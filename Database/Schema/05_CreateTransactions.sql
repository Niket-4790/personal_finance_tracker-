USE PersonalFinanceTracker;
GO

CREATE TABLE dbo.Transactions
(
    Id INT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_Transactions PRIMARY KEY,

    UserId INT NOT NULL,

    AccountId INT NOT NULL,

    CategoryId INT NOT NULL,

    Amount DECIMAL(18,2) NOT NULL,

    Description NVARCHAR(500) NULL,

    TransactionDate DATE NOT NULL,

    TransactionType NVARCHAR(20) NOT NULL,

    CreatedAt DATETIME2 NOT NULL
        CONSTRAINT DF_Transactions_CreatedAt
        DEFAULT (SYSUTCDATETIME()),

    CONSTRAINT FK_Transactions_Users
        FOREIGN KEY (UserId)
        REFERENCES dbo.Users(Id),

    CONSTRAINT FK_Transactions_Accounts
        FOREIGN KEY (AccountId)
        REFERENCES dbo.Accounts(Id),

    CONSTRAINT FK_Transactions_Categories
        FOREIGN KEY (CategoryId)
        REFERENCES dbo.Categories(Id),

    CONSTRAINT CK_Transactions_Amount
        CHECK (Amount > 0),

    CONSTRAINT CK_Transactions_Type
        CHECK (TransactionType IN ('Income', 'Expense'))
);
GO