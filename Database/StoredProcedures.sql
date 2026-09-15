-- Personal Finance Tracker - Stored Procedures
USE PersonalFinanceTracker;
GO

-- ========== USERS ==========
-- Auth/user-management procs. AuthService (register/validate credentials) and
-- UserService (admin-only list/promote/deactivate) are the only C# callers -
-- kept separate the same way FinTrack keeps those two responsibilities apart,
-- so an admin-power action never accidentally ends up reachable from an
-- anonymous-facing page.

CREATE OR ALTER PROCEDURE dbo.usp_User_Create
    @Name         NVARCHAR(100),
    @Email        NVARCHAR(256),
    @PasswordHash NVARCHAR(MAX),
    @Role         NVARCHAR(20),
    @NewId        INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Users (Name, Email, PasswordHash, Role)
    VALUES (@Name, @Email, @PasswordHash, @Role);
    SET @NewId = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_User_GetByEmail
    @Email NVARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Name, Email, PasswordHash, Role, IsActive, CreatedAt, GoogleSubjectId
    FROM dbo.Users
    WHERE Email = @Email;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_User_GetById
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Name, Email, Role, IsActive, CreatedAt, GoogleSubjectId
    FROM dbo.Users
    WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_User_GetAll
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Name, Email, Role, IsActive, CreatedAt, GoogleSubjectId
    FROM dbo.Users
    ORDER BY Name;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_User_SetRole
    @Id   INT,
    @Role NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Users SET Role = @Role WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_User_SetActive
    @Id       INT,
    @IsActive BIT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Users SET IsActive = @IsActive WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_User_GetAdminCount
    @Count INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT @Count = COUNT(*) FROM dbo.Users WHERE Role = 'Admin';
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_User_GetActiveAdminCount
    @Count INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT @Count = COUNT(*) FROM dbo.Users WHERE Role = 'Admin' AND IsActive = 1;
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

-- ========== ACCOUNTS ==========
-- Personal data - always scoped to the owning @UserId, both to read only
-- "my" accounts and so a guessed Id can never touch someone else's account.

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

-- ========== CATEGORIES ==========

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

-- ========== TRANSACTIONS ==========

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

-- ========== DASHBOARD & REPORTS (per-user) ==========

CREATE OR ALTER PROCEDURE dbo.usp_Dashboard_GetSummary
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @MonthStart DATE = DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1);
    DECLARE @MonthEnd   DATE = EOMONTH(GETDATE());

    SELECT
        (SELECT ISNULL(SUM(Balance), 0) FROM dbo.Accounts WHERE UserId = @UserId) AS TotalBalance,
        (SELECT ISNULL(SUM(Amount), 0) FROM dbo.Transactions
         WHERE UserId = @UserId AND TransactionType = 'Income'
           AND TransactionDate BETWEEN @MonthStart AND @MonthEnd) AS MonthlyIncome,
        (SELECT ISNULL(SUM(Amount), 0) FROM dbo.Transactions
         WHERE UserId = @UserId AND TransactionType = 'Expense'
           AND TransactionDate BETWEEN @MonthStart AND @MonthEnd) AS MonthlyExpenses,
        (SELECT COUNT(*) FROM dbo.Transactions
         WHERE UserId = @UserId AND TransactionDate BETWEEN @MonthStart AND @MonthEnd) AS TransactionCount;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Report_GetMonthlyTotals
    @UserId INT,
    @MonthsBack INT = 6
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH MonthSeries AS (
        SELECT 0 AS MonthOffset
        UNION ALL
        SELECT MonthOffset + 1
        FROM MonthSeries
        WHERE MonthOffset + 1 < @MonthsBack
    )
    SELECT
        YEAR(DATEADD(MONTH, -ms.MonthOffset, GETDATE())) AS [Year],
        MONTH(DATEADD(MONTH, -ms.MonthOffset, GETDATE())) AS [Month],
        DATENAME(MONTH, DATEADD(MONTH, -ms.MonthOffset, GETDATE())) AS MonthName,
        ISNULL(SUM(CASE WHEN t.TransactionType = 'Income'  THEN t.Amount END), 0) AS TotalIncome,
        ISNULL(SUM(CASE WHEN t.TransactionType = 'Expense' THEN t.Amount END), 0) AS TotalExpenses,
        ISNULL(SUM(CASE WHEN t.TransactionType = 'Income'  THEN t.Amount END), 0)
            - ISNULL(SUM(CASE WHEN t.TransactionType = 'Expense' THEN t.Amount END), 0) AS NetSavings
    FROM MonthSeries ms
    LEFT JOIN dbo.Transactions t
        ON t.UserId = @UserId
       AND YEAR(t.TransactionDate)  = YEAR(DATEADD(MONTH, -ms.MonthOffset, GETDATE()))
       AND MONTH(t.TransactionDate) = MONTH(DATEADD(MONTH, -ms.MonthOffset, GETDATE()))
    GROUP BY ms.MonthOffset,
             YEAR(DATEADD(MONTH, -ms.MonthOffset, GETDATE())),
             MONTH(DATEADD(MONTH, -ms.MonthOffset, GETDATE())),
             DATENAME(MONTH, DATEADD(MONTH, -ms.MonthOffset, GETDATE()))
    ORDER BY [Year] DESC, [Month] DESC
    OPTION (MAXRECURSION 100);
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Report_GetCategoryBreakdown
    @UserId INT,
    @Month INT = NULL,
    @Year  INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @Month IS NULL SET @Month = MONTH(GETDATE());
    IF @Year  IS NULL SET @Year  = YEAR(GETDATE());

    SELECT
        c.Name AS CategoryName,
        c.Type AS CategoryType,
        c.Color AS CategoryColor,
        ISNULL(SUM(t.Amount), 0) AS TotalAmount
    FROM dbo.Categories c
    LEFT JOIN dbo.Transactions t
        ON t.CategoryId = c.Id
       AND MONTH(t.TransactionDate) = @Month
       AND YEAR(t.TransactionDate)  = @Year
    WHERE c.UserId = @UserId
    GROUP BY c.Id, c.Name, c.Type, c.Color
    HAVING ISNULL(SUM(t.Amount), 0) > 0
    ORDER BY TotalAmount DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Report_CalculateSavingsRate
    @UserId INT,
    @Month INT = NULL,
    @Year  INT = NULL,
    @SavingsRate DECIMAL(10,2) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF @Month IS NULL SET @Month = MONTH(GETDATE());
    IF @Year  IS NULL SET @Year  = YEAR(GETDATE());

    DECLARE @Income  DECIMAL(18,2);
    DECLARE @Expense DECIMAL(18,2);

    SELECT @Income = ISNULL(SUM(Amount), 0)
    FROM dbo.Transactions
    WHERE UserId = @UserId
      AND TransactionType = 'Income'
      AND MONTH(TransactionDate) = @Month
      AND YEAR(TransactionDate)  = @Year;

    SELECT @Expense = ISNULL(SUM(Amount), 0)
    FROM dbo.Transactions
    WHERE UserId = @UserId
      AND TransactionType = 'Expense'
      AND MONTH(TransactionDate) = @Month
      AND YEAR(TransactionDate)  = @Year;

    SET @SavingsRate = CASE
        WHEN @Income = 0 THEN 0
        ELSE ROUND(((@Income - @Expense) / @Income) * 100, 2)
    END;
END
GO

-- ========== DASHBOARD & REPORTS (Admin, all users) ==========
-- Admin manages the system and observes aggregate data - these never take a
-- @UserId, since "all users" is the whole point of the Admin view.

CREATE OR ALTER PROCEDURE dbo.usp_Dashboard_GetSystemSummary
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @MonthStart DATE = DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1);
    DECLARE @MonthEnd   DATE = EOMONTH(GETDATE());

    SELECT
        (SELECT COUNT(*) FROM dbo.Users WHERE Role = 'User' AND IsActive = 1) AS TotalUsers,
        (SELECT ISNULL(SUM(Balance), 0) FROM dbo.Accounts) AS TotalBalance,
        (SELECT ISNULL(SUM(Amount), 0) FROM dbo.Transactions
         WHERE TransactionType = 'Income'
           AND TransactionDate BETWEEN @MonthStart AND @MonthEnd) AS MonthlyIncome,
        (SELECT ISNULL(SUM(Amount), 0) FROM dbo.Transactions
         WHERE TransactionType = 'Expense'
           AND TransactionDate BETWEEN @MonthStart AND @MonthEnd) AS MonthlyExpenses,
        (SELECT COUNT(*) FROM dbo.Transactions
         WHERE TransactionDate BETWEEN @MonthStart AND @MonthEnd) AS TransactionCount;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Report_GetMonthlyTotalsAllUsers
    @MonthsBack INT = 6
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH MonthSeries AS (
        SELECT 0 AS MonthOffset
        UNION ALL
        SELECT MonthOffset + 1
        FROM MonthSeries
        WHERE MonthOffset + 1 < @MonthsBack
    )
    SELECT
        YEAR(DATEADD(MONTH, -ms.MonthOffset, GETDATE())) AS [Year],
        MONTH(DATEADD(MONTH, -ms.MonthOffset, GETDATE())) AS [Month],
        DATENAME(MONTH, DATEADD(MONTH, -ms.MonthOffset, GETDATE())) AS MonthName,
        ISNULL(SUM(CASE WHEN t.TransactionType = 'Income'  THEN t.Amount END), 0) AS TotalIncome,
        ISNULL(SUM(CASE WHEN t.TransactionType = 'Expense' THEN t.Amount END), 0) AS TotalExpenses,
        ISNULL(SUM(CASE WHEN t.TransactionType = 'Income'  THEN t.Amount END), 0)
            - ISNULL(SUM(CASE WHEN t.TransactionType = 'Expense' THEN t.Amount END), 0) AS NetSavings
    FROM MonthSeries ms
    LEFT JOIN dbo.Transactions t
        ON YEAR(t.TransactionDate)  = YEAR(DATEADD(MONTH, -ms.MonthOffset, GETDATE()))
       AND MONTH(t.TransactionDate) = MONTH(DATEADD(MONTH, -ms.MonthOffset, GETDATE()))
    GROUP BY ms.MonthOffset,
             YEAR(DATEADD(MONTH, -ms.MonthOffset, GETDATE())),
             MONTH(DATEADD(MONTH, -ms.MonthOffset, GETDATE())),
             DATENAME(MONTH, DATEADD(MONTH, -ms.MonthOffset, GETDATE()))
    ORDER BY [Year] DESC, [Month] DESC
    OPTION (MAXRECURSION 100);
END
GO

-- Categories are per-user, so two different users can each have their own
-- "Groceries" category - grouped by Name/Type here (MIN(Color) as a single
-- representative swatch) rather than by Id, so the same category name
-- aggregates across every user's data instead of showing one row per user.
CREATE OR ALTER PROCEDURE dbo.usp_Report_GetCategoryBreakdownAllUsers
    @Month INT = NULL,
    @Year  INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @Month IS NULL SET @Month = MONTH(GETDATE());
    IF @Year  IS NULL SET @Year  = YEAR(GETDATE());

    SELECT
        c.Name AS CategoryName,
        c.Type AS CategoryType,
        MIN(c.Color) AS CategoryColor,
        ISNULL(SUM(t.Amount), 0) AS TotalAmount
    FROM dbo.Categories c
    LEFT JOIN dbo.Transactions t
        ON t.CategoryId = c.Id
       AND MONTH(t.TransactionDate) = @Month
       AND YEAR(t.TransactionDate)  = @Year
    GROUP BY c.Name, c.Type
    HAVING ISNULL(SUM(t.Amount), 0) > 0
    ORDER BY TotalAmount DESC;
END
GO
