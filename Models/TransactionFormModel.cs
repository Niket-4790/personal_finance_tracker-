namespace PersonalFinanceTracker.Models;

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