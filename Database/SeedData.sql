-- Personal Finance Tracker - Seed Data
USE PersonalFinanceTracker;
GO

-- Create only the Admin user.
-- Password: Admin@123

SET IDENTITY_INSERT dbo.Users ON;

INSERT INTO dbo.Users
(
    Id,
    Name,
    Email,
    PasswordHash,
    Role,
    IsActive
)
VALUES
(
    1,
    N'Admin',
    N'admin@financetrack.local',
    N'AQAAAAEAACcQAAAAEM1djq0lZXo+dJOfS8WwF4HUwdEUdqyevXA+6Dl3FD7FDtB2if2PUGwb6IluqmiegA==',
    N'Admin',
    1
);

SET IDENTITY_INSERT dbo.Users OFF;
GO