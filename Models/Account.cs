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