USE PersonalFinanceTracker;
GO

CREATE TABLE dbo.Payments
(
    Id INT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_Payments PRIMARY KEY,

    UserId INT NOT NULL,

    RazorpayOrderId NVARCHAR(100) NOT NULL,

    RazorpayPaymentId NVARCHAR(100) NULL,

    Amount DECIMAL(10,2) NOT NULL,

    Status NVARCHAR(20) NOT NULL
        CONSTRAINT DF_Payments_Status
        DEFAULT ('Created'),

    PaidAt DATETIME2 NULL,

    CreatedAt DATETIME2 NOT NULL
        CONSTRAINT DF_Payments_CreatedAt
        DEFAULT (SYSUTCDATETIME()),

    CONSTRAINT FK_Payments_Users
        FOREIGN KEY (UserId)
        REFERENCES dbo.Users(Id),

    CONSTRAINT CK_Payments_Status
        CHECK (Status IN ('Created', 'Paid', 'Failed'))
);
GO

CREATE UNIQUE INDEX UX_Payments_RazorpayOrderId
ON dbo.Payments(RazorpayOrderId);
GO