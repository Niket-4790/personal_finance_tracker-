USE PersonalFinanceTracker;
GO

CREATE TABLE dbo.Notifications
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    Message NVARCHAR(500) NOT NULL,
    IsRead BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_Notifications_Users
        FOREIGN KEY (UserId)
        REFERENCES dbo.Users(Id)
);