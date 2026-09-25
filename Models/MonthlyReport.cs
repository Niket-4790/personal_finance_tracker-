namespace PersonalFinanceTracker.Models;

public class MonthlyReport
{
    public int Year { get; set; }

    public int Month { get; set; }

    public string MonthName { get; set; } = string.Empty;

    public decimal TotalIncome { get; set; }

    public decimal TotalExpenses { get; set; }

    public decimal NetSavings { get; set; }
}