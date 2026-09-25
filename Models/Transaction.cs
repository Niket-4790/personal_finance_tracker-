namespace PersonalFinanceTracker.Models;

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

    /// <summary>
    /// Only populated by the admin, all-users query.
    /// </summary>
    public string? UserName { get; set; }
}