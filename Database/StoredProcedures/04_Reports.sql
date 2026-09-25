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