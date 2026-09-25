USE PersonalFinanceTracker;
GO

CREATE TABLE dbo.Budgets
(
    Id INT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_Budgets PRIMARY KEY,

    UserId INT NOT NULL,

    CategoryId INT NOT NULL,

    BudgetMonth DATE NOT NULL,

    Amount DECIMAL(18,2) NOT NULL,

    CreatedAt DATETIME2 NOT NULL
        CONSTRAINT DF_Budgets_CreatedAt
        DEFAULT (SYSUTCDATETIME()),

    CONSTRAINT FK_Budgets_Users
        FOREIGN KEY (UserId)
        REFERENCES dbo.Users(Id),

    CONSTRAINT FK_Budgets_Categories
        FOREIGN KEY (CategoryId)
        REFERENCES dbo.Categories(Id),

    CONSTRAINT CK_Budgets_Amount
        CHECK (Amount > 0),

    CONSTRAINT UQ_Budgets_User_Category_Month
        UNIQUE (UserId, CategoryId, BudgetMonth)
);
GO