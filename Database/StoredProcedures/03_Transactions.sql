CREATE OR ALTER PROCEDURE dbo.usp_Transaction_GetAll
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        t.Id,
        t.UserId,
        t.AccountId,
        a.Name AS AccountName,
        t.CategoryId,
        c.Name AS CategoryName,
        c.Color AS CategoryColor,
        t.Amount,
        t.Description,
        t.TransactionDate,
        t.TransactionType,
        t.CreatedAt
    FROM dbo.Transactions t
    INNER JOIN dbo.Accounts a ON a.Id = t.AccountId
    INNER JOIN dbo.Categories c ON c.Id = t.CategoryId
    WHERE t.UserId = @UserId
    ORDER BY t.TransactionDate DESC, t.Id DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Transaction_GetById
    @Id INT,
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        t.Id,
        t.UserId,
        t.AccountId,
        a.Name AS AccountName,
        t.CategoryId,
        c.Name AS CategoryName,
        c.Color AS CategoryColor,
        t.Amount,
        t.Description,
        t.TransactionDate,
        t.TransactionType,
        t.CreatedAt
    FROM dbo.Transactions t
    INNER JOIN dbo.Accounts a ON a.Id = t.AccountId
    INNER JOIN dbo.Categories c ON c.Id = t.CategoryId
    WHERE t.Id = @Id AND t.UserId = @UserId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Transaction_Insert
    @UserId          INT,
    @AccountId       INT,
    @CategoryId      INT,
    @Amount          DECIMAL(18,2),
    @Description     NVARCHAR(500),
    @TransactionDate DATE,
    @TransactionType NVARCHAR(20),
    @NewId           INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    -- Defense in depth: even if a caller passed someone else's AccountId/CategoryId,
    -- it can never be attached to this user's transaction.
    IF NOT EXISTS (SELECT 1 FROM dbo.Accounts WHERE Id = @AccountId AND UserId = @UserId)
    BEGIN
        RAISERROR('Account does not belong to this user.', 16, 1);
        RETURN;
    END
    IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE Id = @CategoryId AND UserId = @UserId)
    BEGIN
        RAISERROR('Category does not belong to this user.', 16, 1);
        RETURN;
    END

    BEGIN TRANSACTION;
    BEGIN TRY
        INSERT INTO dbo.Transactions (UserId, AccountId, CategoryId, Amount, Description, TransactionDate, TransactionType)
        VALUES (@UserId, @AccountId, @CategoryId, @Amount, @Description, @TransactionDate, @TransactionType);
        SET @NewId = SCOPE_IDENTITY();

        EXEC dbo.usp_Account_RecalculateBalance @AccountId = @AccountId;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Transaction_Update
    @Id              INT,
    @UserId          INT,
    @AccountId       INT,
    @CategoryId      INT,
    @Amount          DECIMAL(18,2),
    @Description     NVARCHAR(500),
    @TransactionDate DATE,
    @TransactionType NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.Accounts WHERE Id = @AccountId AND UserId = @UserId)
    BEGIN
        RAISERROR('Account does not belong to this user.', 16, 1);
        RETURN;
    END
    IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE Id = @CategoryId AND UserId = @UserId)
    BEGIN
        RAISERROR('Category does not belong to this user.', 16, 1);
        RETURN;
    END

    DECLARE @OldAccountId INT;
    SELECT @OldAccountId = AccountId FROM dbo.Transactions WHERE Id = @Id AND UserId = @UserId;

    IF @OldAccountId IS NULL
    BEGIN
        RAISERROR('Transaction does not belong to this user.', 16, 1);
        RETURN;
    END

    BEGIN TRANSACTION;
    BEGIN TRY
        UPDATE dbo.Transactions
        SET AccountId = @AccountId,
            CategoryId = @CategoryId,
            Amount = @Amount,
            Description = @Description,
            TransactionDate = @TransactionDate,
            TransactionType = @TransactionType
        WHERE Id = @Id AND UserId = @UserId;

        EXEC dbo.usp_Account_RecalculateBalance @AccountId = @OldAccountId;
        IF @OldAccountId <> @AccountId
            EXEC dbo.usp_Account_RecalculateBalance @AccountId = @AccountId;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Transaction_Delete
    @Id INT,
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @AccountId INT;

    SELECT @AccountId = AccountId FROM dbo.Transactions WHERE Id = @Id AND UserId = @UserId;

    IF @AccountId IS NULL
    BEGIN
        RAISERROR('Transaction does not belong to this user.', 16, 1);
        RETURN;
    END

    BEGIN TRANSACTION;
    BEGIN TRY
        DELETE FROM dbo.Transactions WHERE Id = @Id AND UserId = @UserId;
        EXEC dbo.usp_Account_RecalculateBalance @AccountId = @AccountId;
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- Admin, read-only, all users - mirrors FinTrack's sp_GetTransactionsForAdmin:
-- an Admin has no transactions of its own, so this always shows every User's
-- data with a UserName column, never filtered to "the logged-in admin".
CREATE OR ALTER PROCEDURE dbo.usp_Transaction_GetAllForAdmin
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        t.Id,
        t.UserId,
        u.Name AS UserName,
        t.AccountId,
        a.Name AS AccountName,
        t.CategoryId,
        c.Name AS CategoryName,
        c.Color AS CategoryColor,
        t.Amount,
        t.Description,
        t.TransactionDate,
        t.TransactionType,
        t.CreatedAt
    FROM dbo.Transactions t
    INNER JOIN dbo.Accounts a ON a.Id = t.AccountId
    INNER JOIN dbo.Categories c ON c.Id = t.CategoryId
    INNER JOIN dbo.Users u ON u.Id = t.UserId
    ORDER BY t.TransactionDate DESC, t.Id DESC;
END
GO