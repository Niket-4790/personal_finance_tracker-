USE PersonalFinanceTracker;
GO

CREATE TABLE dbo.Categories
(
    Id INT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_Categories PRIMARY KEY,

    UserId INT NOT NULL,

    Name NVARCHAR(100) NOT NULL,

    Type NVARCHAR(20) NOT NULL,

    Color NVARCHAR(7) NOT NULL
        CONSTRAINT DF_Categories_Color
        DEFAULT ('#6366f1'),

    CONSTRAINT FK_Categories_Users
        FOREIGN KEY (UserId)
        REFERENCES dbo.Users(Id),

    CONSTRAINT CK_Categories_Type
        CHECK (Type IN ('Income', 'Expense'))
);
GO