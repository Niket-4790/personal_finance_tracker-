namespace PersonalFinanceTracker.Models;

public class DashboardSummary
{
    public decimal TotalBalance { get; set; }

    public decimal MonthlyIncome { get; set; }

    public decimal MonthlyExpenses { get; set; }

    public int TransactionCount { get; set; }

    public decimal NetMonthly => MonthlyIncome - MonthlyExpenses;
}