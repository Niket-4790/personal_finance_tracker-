CREATE OR ALTER PROCEDURE dbo.usp_Budget_GetAll
    @UserId      INT,
    @BudgetMonth DATE
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        b.Id,
        b.UserId,
        b.CategoryId,
        c.Name AS CategoryName,
        c.Color AS CategoryColor,
        b.BudgetMonth,
        b.Amount,
        b.CreatedAt,

        ISNULL(
            (
                SELECT SUM(t.Amount)
                FROM dbo.Transactions t
                WHERE t.UserId = b.UserId
                  AND t.CategoryId = b.CategoryId
                  AND t.TransactionType = 'Expense'
                  AND t.TransactionDate >= b.BudgetMonth
                  AND t.TransactionDate < DATEADD(MONTH, 1, b.BudgetMonth)
            ),
            0
        ) AS SpentAmount

    FROM dbo.Budgets b

    INNER JOIN dbo.Categories c
        ON c.Id = b.CategoryId

    WHERE b.UserId = @UserId
      AND b.BudgetMonth = @BudgetMonth

    ORDER BY c.Name;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Budget_GetById
    @Id     INT,
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        b.Id,
        b.UserId,
        b.CategoryId,
        c.Name AS CategoryName,
        c.Color AS CategoryColor,
        b.BudgetMonth,
        b.Amount,
        b.CreatedAt,

        ISNULL(
            (
                SELECT SUM(t.Amount)
                FROM dbo.Transactions t
                WHERE t.UserId = b.UserId
                  AND t.CategoryId = b.CategoryId
                  AND t.TransactionType = 'Expense'
                  AND t.TransactionDate >= b.BudgetMonth
                  AND t.TransactionDate < DATEADD(MONTH, 1, b.BudgetMonth)
            ),
            0
        ) AS SpentAmount

    FROM dbo.Budgets b

    INNER JOIN dbo.Categories c
        ON c.Id = b.CategoryId

    WHERE b.Id = @Id
      AND b.UserId = @UserId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Budget_Insert
    @UserId      INT,
    @CategoryId  INT,
    @BudgetMonth DATE,
    @Amount      DECIMAL(18,2),
    @NewId       INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    -- Make sure the category belongs to this user
    -- and is an Expense category.
    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.Categories
        WHERE Id = @CategoryId
          AND UserId = @UserId
          AND Type = 'Expense'
    )
    BEGIN
        RAISERROR('Invalid expense category for this user.', 16, 1);
        RETURN;
    END;

    -- Normalize the month to the first day.
    SET @BudgetMonth = DATEFROMPARTS(
        YEAR(@BudgetMonth),
        MONTH(@BudgetMonth),
        1
    );

    -- Prevent duplicate budget.
    IF EXISTS
    (
        SELECT 1
        FROM dbo.Budgets
        WHERE UserId = @UserId
          AND CategoryId = @CategoryId
          AND BudgetMonth = @BudgetMonth
    )
    BEGIN
        RAISERROR('A budget already exists for this category and month.', 16, 1);
        RETURN;
    END;

    INSERT INTO dbo.Budgets
    (
        UserId,
        CategoryId,
        BudgetMonth,
        Amount
    )
    VALUES
    (
        @UserId,
        @CategoryId,
        @BudgetMonth,
        @Amount
    );

    SET @NewId = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Budget_Update
    @Id          INT,
    @UserId      INT,
    @CategoryId  INT,
    @BudgetMonth DATE,
    @Amount      DECIMAL(18,2)
AS
BEGIN
    SET NOCOUNT ON;

    -- Make sure the category belongs to this user
    -- and is an Expense category.
    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.Categories
        WHERE Id = @CategoryId
          AND UserId = @UserId
          AND Type = 'Expense'
    )
    BEGIN
        RAISERROR('Invalid expense category for this user.', 16, 1);
        RETURN;
    END;

    -- Normalize the month to the first day.
    SET @BudgetMonth = DATEFROMPARTS(
        YEAR(@BudgetMonth),
        MONTH(@BudgetMonth),
        1
    );

    -- Make sure the budget belongs to this user.
    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.Budgets
        WHERE Id = @Id
          AND UserId = @UserId
    )
    BEGIN
        RAISERROR('Budget not found.', 16, 1);
        RETURN;
    END;

    -- Prevent duplicate budget for another existing record.
    IF EXISTS
    (
        SELECT 1
        FROM dbo.Budgets
        WHERE UserId = @UserId
          AND CategoryId = @CategoryId
          AND BudgetMonth = @BudgetMonth
          AND Id <> @Id
    )
    BEGIN
        RAISERROR('A budget already exists for this category and month.', 16, 1);
        RETURN;
    END;

    UPDATE dbo.Budgets
    SET
        CategoryId = @CategoryId,
        BudgetMonth = @BudgetMonth,
        Amount = @Amount
    WHERE Id = @Id
      AND UserId = @UserId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Budget_Delete
    @Id     INT,
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM dbo.Budgets
    WHERE Id = @Id
      AND UserId = @UserId;
END
GO