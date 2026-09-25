CREATE OR ALTER PROCEDURE dbo.usp_Notification_GetAll
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        Id,
        UserId,
        Title,
        Message,
        IsRead,
        CreatedAt
    FROM dbo.Notifications
    WHERE UserId = @UserId
    ORDER BY CreatedAt DESC;
END
GO


CREATE OR ALTER PROCEDURE dbo.usp_Notification_GetUnreadCount
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT COUNT(*)
    FROM dbo.Notifications
    WHERE UserId = @UserId
      AND IsRead = 0;
END
GO


CREATE OR ALTER PROCEDURE dbo.usp_Notification_Insert
    @UserId INT,
    @Title NVARCHAR(200),
    @Message NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.Notifications
    (
        UserId,
        Title,
        Message
    )
    VALUES
    (
        @UserId,
        @Title,
        @Message
    );

    SELECT CAST(SCOPE_IDENTITY() AS INT);
END
GO


CREATE OR ALTER PROCEDURE dbo.usp_Notification_MarkAsRead
    @Id INT,
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Notifications
    SET IsRead = 1
    WHERE Id = @Id
      AND UserId = @UserId;
END
GO


CREATE OR ALTER PROCEDURE dbo.usp_Notification_MarkAllAsRead
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Notifications
    SET IsRead = 1
    WHERE UserId = @UserId
      AND IsRead = 0;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Notification_Delete
    @Id INT,
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM dbo.Notifications
    WHERE Id = @Id
      AND UserId = @UserId;
END
GO