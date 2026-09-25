namespace PersonalFinanceTracker.Models;

/// <summary>
/// The Admin's view of the whole system, in place of a per-user
/// DashboardSummary.
///
/// An Admin has no accounts/transactions of its own, so it never gets
/// a regular DashboardSummary, only aggregate counts across every User.
/// </summary>
public class SystemSummary
{
    public int TotalUsers { get; set; }

    public decimal TotalBalance { get; set; }

    public decimal MonthlyIncome { get; set; }

    public decimal MonthlyExpenses { get; set; }

    public int TransactionCount { get; set; }

    public decimal NetMonthly =>
        MonthlyIncome - MonthlyExpenses;
}