namespace PersonalFinanceTracker.Models;

public class Budget
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public string CategoryColor { get; set; } = "#6366f1";

    public DateTime BudgetMonth { get; set; }

    public decimal Amount { get; set; }

    public decimal SpentAmount { get; set; }

    public decimal RemainingAmount { get; set; }

    public decimal PercentageUsed { get; set; }

    public DateTime CreatedAt { get; set; }
}