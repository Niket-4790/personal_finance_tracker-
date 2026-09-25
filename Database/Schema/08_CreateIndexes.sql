USE PersonalFinanceTracker;
GO

CREATE INDEX IX_Accounts_UserId
ON dbo.Accounts(UserId);

CREATE INDEX IX_Categories_UserId
ON dbo.Categories(UserId);

CREATE INDEX IX_Transactions_UserId_Date
ON dbo.Transactions(UserId, TransactionDate DESC);

CREATE INDEX IX_Transactions_AccountId
ON dbo.Transactions(AccountId);

CREATE INDEX IX_Transactions_CategoryId
ON dbo.Transactions(CategoryId);

CREATE INDEX IX_Payments_UserId
ON dbo.Payments(UserId);

CREATE INDEX IX_Notifications_UserId
ON dbo.Notifications(UserId);
GO