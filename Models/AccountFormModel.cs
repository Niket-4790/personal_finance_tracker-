namespace PersonalFinanceTracker.Models;

public class AccountFormModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string AccountType { get; set; } = "Checking";

    public decimal InitialBalance { get; set; }
}