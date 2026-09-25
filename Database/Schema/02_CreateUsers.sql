USE PersonalFinanceTracker;
GO

CREATE TABLE dbo.Users
(
    Id INT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_Users PRIMARY KEY,

    Name NVARCHAR(100) NOT NULL,

    Email NVARCHAR(256) NOT NULL,

    PasswordHash NVARCHAR(MAX) NULL,

    GoogleSubjectId NVARCHAR(255) NULL,

    Role NVARCHAR(20) NOT NULL
        CONSTRAINT DF_Users_Role
        DEFAULT ('User'),

    IsActive BIT NOT NULL
        CONSTRAINT DF_Users_IsActive
        DEFAULT (1),

    CreatedAt DATETIME2 NOT NULL
        CONSTRAINT DF_Users_CreatedAt
        DEFAULT (SYSUTCDATETIME()),

    CONSTRAINT UQ_Users_Email
        UNIQUE (Email),

    CONSTRAINT UQ_Users_GoogleSubjectId
        UNIQUE (GoogleSubjectId),

    CONSTRAINT CK_Users_Role
        CHECK (Role IN ('Admin', 'User')),

    CONSTRAINT CK_Users_Name_NotEmpty
        CHECK (LEN(LTRIM(RTRIM(Name))) > 0),

    CONSTRAINT CK_Users_Email_NotEmpty
        CHECK (LEN(LTRIM(RTRIM(Email))) > 0)
);
GO