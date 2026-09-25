USE PersonalFinanceTracker;
GO

CREATE OR ALTER PROCEDURE dbo.usp_User_Create
    @Name         NVARCHAR(100),
    @Email        NVARCHAR(256),
    @PasswordHash NVARCHAR(MAX),
    @Role         NVARCHAR(20),
    @NewId        INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.Users
    (
        Name,
        Email,
        PasswordHash,
        Role
    )
    VALUES
    (
        @Name,
        @Email,
        @PasswordHash,
        @Role
    );

    SET @NewId = SCOPE_IDENTITY();
END
GO


CREATE OR ALTER PROCEDURE dbo.usp_User_GetByEmail
    @Email NVARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        Id,
        Name,
        Email,
        PasswordHash,
        Role,
        IsActive,
        CreatedAt,
        GoogleSubjectId
    FROM dbo.Users
    WHERE Email = @Email;
END
GO


CREATE OR ALTER PROCEDURE dbo.usp_User_GetById
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        Id,
        Name,
        Email,
        Role,
        IsActive,
        CreatedAt,
        GoogleSubjectId
    FROM dbo.Users
    WHERE Id = @Id;
END
GO


CREATE OR ALTER PROCEDURE dbo.usp_User_GetAll
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        Id,
        Name,
        Email,
        Role,
        IsActive,
        CreatedAt,
        GoogleSubjectId
    FROM dbo.Users
    ORDER BY Name;
END
GO


CREATE OR ALTER PROCEDURE dbo.usp_User_SetRole
    @Id INT,
    @Role NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Users
    SET Role = @Role
    WHERE Id = @Id;
END
GO


CREATE OR ALTER PROCEDURE dbo.usp_User_SetActive
    @Id INT,
    @IsActive BIT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Users
    SET IsActive = @IsActive
    WHERE Id = @Id;
END
GO


CREATE OR ALTER PROCEDURE dbo.usp_User_GetAdminCount
    @Count INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT @Count = COUNT(*)
    FROM dbo.Users
    WHERE Role = 'Admin';
END
GO


CREATE OR ALTER PROCEDURE dbo.usp_User_GetActiveAdminCount
    @Count INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT @Count = COUNT(*)
    FROM dbo.Users
    WHERE Role = 'Admin'
      AND IsActive = 1;
END
GO


CREATE OR ALTER PROCEDURE dbo.usp_User_GetByGoogleSubjectId
    @GoogleSubjectId NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        Id,
        Name,
        Email,
        Role,
        IsActive,
        CreatedAt,
        GoogleSubjectId
    FROM dbo.Users
    WHERE GoogleSubjectId = @GoogleSubjectId;
END
GO


CREATE OR ALTER PROCEDURE dbo.usp_User_CreateGoogle
    @Name NVARCHAR(100),
    @Email NVARCHAR(256),
    @GoogleSubjectId NVARCHAR(255),
    @NewId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.Users
    (
        Name,
        Email,
        PasswordHash,
        GoogleSubjectId,
        Role
    )
    VALUES
    (
        @Name,
        @Email,
        NULL,
        @GoogleSubjectId,
        'User'
    );

    SET @NewId = CONVERT(INT, SCOPE_IDENTITY());
END
GO


CREATE OR ALTER PROCEDURE dbo.usp_User_SetGoogleSubjectId
    @Id INT,
    @GoogleSubjectId NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Users
    SET GoogleSubjectId = @GoogleSubjectId
    WHERE Id = @Id;
END
GO