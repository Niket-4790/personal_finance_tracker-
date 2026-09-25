CREATE OR ALTER PROCEDURE dbo.usp_Account_GetAll
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, UserId, Name, AccountType, InitialBalance, Balance, CreatedAt
    FROM dbo.Accounts
    WHERE UserId = @UserId
    ORDER BY Name;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Account_GetById
    @Id INT,
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, UserId, Name, AccountType, InitialBalance, Balance, CreatedAt
    FROM dbo.Accounts
    WHERE Id = @Id AND UserId = @UserId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Account_Insert
    @UserId        INT,
    @Name          NVARCHAR(100),
    @AccountType   NVARCHAR(50),
    @InitialBalance DECIMAL(18,2),
    @NewId         INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.Accounts
    (
        UserId,
        Name,
        AccountType,
        InitialBalance,
        Balance
    )
    VALUES
    (
        @UserId,
        @Name,
        @AccountType,
        @InitialBalance,
        @InitialBalance
    );

    SET @NewId = SCOPE_IDENTITY();
END 
GO

CREATE OR ALTER PROCEDURE dbo.usp_Account_Update
    @Id          INT,
    @UserId      INT,
    @Name        NVARCHAR(100),
    @AccountType NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Accounts
    SET Name = @Name, AccountType = @AccountType
    WHERE Id = @Id AND UserId = @UserId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Account_Delete
    @Id INT,
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM dbo.Transactions WHERE AccountId = @Id AND UserId = @UserId)
    BEGIN
        RAISERROR('Cannot delete account with existing transactions.', 16, 1);
        RETURN;
    END
    DELETE FROM dbo.Accounts WHERE Id = @Id AND UserId = @UserId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Account_RecalculateBalance
    @AccountId INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE a
    SET Balance =
        a.InitialBalance
        +
        ISNULL(
            (
                SELECT SUM(
                    CASE
                        WHEN t.TransactionType = 'Income'
                            THEN t.Amount
                        ELSE -t.Amount
                    END
                )
                FROM dbo.Transactions t
                WHERE t.AccountId = @AccountId
            ),
            0
        )
    FROM dbo.Accounts a
    WHERE a.Id = @AccountId;
END
GO 
GO