CREATE OR ALTER PROCEDURE dbo.usp_Category_GetAll
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, UserId, Name, Type, Color
    FROM dbo.Categories
    WHERE UserId = @UserId
    ORDER BY Type, Name;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Category_GetById
    @Id INT,
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, UserId, Name, Type, Color
    FROM dbo.Categories
    WHERE Id = @Id AND UserId = @UserId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Category_Insert
    @UserId INT,
    @Name   NVARCHAR(100),
    @Type   NVARCHAR(20),
    @Color  NVARCHAR(7),
    @NewId  INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Categories (UserId, Name, Type, Color)
    VALUES (@UserId, @Name, @Type, @Color);
    SET @NewId = SCOPE_IDENTITY();
END
GO