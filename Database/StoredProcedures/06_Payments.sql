CREATE OR ALTER PROCEDURE dbo.usp_Payment_Insert
    @UserId INT,
    @RazorpayOrderId NVARCHAR(100),
    @Amount DECIMAL(10,2),
    @Status NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.Payments
    (
        UserId,
        RazorpayOrderId,
        Amount,
        Status
    )
    VALUES
    (
        @UserId,
        @RazorpayOrderId,
        @Amount,
        @Status
    );

    SELECT CAST(SCOPE_IDENTITY() AS INT);
END
GO


CREATE OR ALTER PROCEDURE dbo.usp_Payment_GetByOrderId
    @RazorpayOrderId NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        Id,
        UserId,
        RazorpayOrderId,
        RazorpayPaymentId,
        Amount,
        Status,
        PaidAt,
        CreatedAt
    FROM dbo.Payments
    WHERE RazorpayOrderId = @RazorpayOrderId;
END
GO


CREATE OR ALTER PROCEDURE dbo.usp_Payment_MarkAsPaid
    @RazorpayOrderId NVARCHAR(100),
    @RazorpayPaymentId NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Payments
    SET
        RazorpayPaymentId = @RazorpayPaymentId,
        Status = 'Paid',
        PaidAt = SYSUTCDATETIME()
    WHERE RazorpayOrderId = @RazorpayOrderId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Payment_GetLatestPaid
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1
        Id,
        UserId,
        RazorpayOrderId,
        RazorpayPaymentId,
        Amount,
        Status,
        PaidAt,
        CreatedAt
    FROM dbo.Payments
    WHERE UserId = @UserId
      AND Status = 'Paid'
    ORDER BY PaidAt DESC;
END
GO