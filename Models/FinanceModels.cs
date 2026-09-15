using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceTracker.Models;

public class Account
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public decimal InitialBalance { get; set; }
    public decimal Balance { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class Category
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Color { get; set; } = "#6366f1";
}

public class Transaction
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int AccountId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryColor { get; set; } = "#6366f1";
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public DateTime TransactionDate { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    /// <summary>Only populated by the admin, all-users query.</summary>
    public string? UserName { get; set; }
}

public class DashboardSummary
{
    public decimal TotalBalance { get; set; }
    public decimal MonthlyIncome { get; set; }
    public decimal MonthlyExpenses { get; set; }
    public int TransactionCount { get; set; }
    public decimal NetMonthly => MonthlyIncome - MonthlyExpenses;
}

/// <summary>
/// The Admin's view of the whole system, in place of a per-user DashboardSummary -
/// an Admin has no accounts/transactions of its own, so it never gets a regular
/// DashboardSummary, only aggregate counts across every User.
/// </summary>
public class SystemSummary
{
    public int TotalUsers { get; set; }
    public decimal TotalBalance { get; set; }
    public decimal MonthlyIncome { get; set; }
    public decimal MonthlyExpenses { get; set; }
    public int TransactionCount { get; set; }
    public decimal NetMonthly => MonthlyIncome - MonthlyExpenses;
}

/// <summary>
/// A user account - Admin manages the system, User owns their own financial data.
/// Never carries the password hash; that's read directly by AuthService from the
/// repository's dedicated GetByEmailWithHashAsync, so it's never accidentally
/// passed around or displayed anywhere else.
/// </summary>
public class AppUser
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = "User";

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public string? GoogleSubjectId { get; set; }
}

/// <summary>
/// Internal shape for IUserRepository.GetByEmailWithHashAsync - carries the
/// PasswordHash so AuthService can verify it, and only AuthService should ever
/// touch this type. Every other consumer (Users.razor, claims, etc.) uses AppUser.
/// </summary>
public class UserCredential
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? PasswordHash { get; set; }

    public string Role { get; set; } = "User";

    public bool IsActive { get; set; } = true;

    public string? GoogleSubjectId { get; set; }
}

public class LoginModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class RegisterModel
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required, Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class MonthlyReport
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetSavings { get; set; }
}

public class CategoryBreakdown
{
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryType { get; set; } = string.Empty;
    public string CategoryColor { get; set; } = "#6366f1";
    public decimal TotalAmount { get; set; }
}

public class TransactionFormModel
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public int CategoryId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; } = DateTime.Today;
    public string TransactionType { get; set; } = "Expense";
}

public class AccountFormModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string AccountType { get; set; } = "Checking";
    public decimal InitialBalance { get; set; }
}
