using PersonalFinanceTracker.Models;

namespace PersonalFinanceTracker.Repositories;

public interface IReportRepository
{
    Task<DashboardSummary?> GetDashboardSummaryAsync(int userId);
    Task<IEnumerable<MonthlyReport>> GetMonthlyTotalsAsync(int userId, int monthsBack = 6);
    Task<IEnumerable<CategoryBreakdown>> GetCategoryBreakdownAsync(int userId, int? month = null, int? year = null);
    Task<decimal> GetSavingsRateAsync(int userId, int? month = null, int? year = null);

    // Admin-only: aggregate across every user.
    Task<SystemSummary?> GetSystemSummaryAsync();
    Task<IEnumerable<MonthlyReport>> GetMonthlyTotalsAllUsersAsync(int monthsBack = 6);
    Task<IEnumerable<CategoryBreakdown>> GetCategoryBreakdownAllUsersAsync(int? month = null, int? year = null);
}
