using PersonalFinanceTracker.Models;

namespace PersonalFinanceTracker.Services;

public interface IReportService
{
    Task<DashboardSummary> GetDashboardSummaryAsync(int userId);
    Task<IEnumerable<MonthlyReport>> GetMonthlyReportsAsync(int userId, int monthsBack = 6);
    Task<IEnumerable<CategoryBreakdown>> GetCategoryBreakdownAsync(int userId, int? month = null, int? year = null);
    Task<decimal> GetSavingsRateAsync(int userId, int? month = null, int? year = null);

    // Admin-only: aggregate across every user.
    Task<SystemSummary> GetSystemSummaryAsync();
    Task<IEnumerable<MonthlyReport>> GetMonthlyReportsAllUsersAsync(int monthsBack = 6);
    Task<IEnumerable<CategoryBreakdown>> GetCategoryBreakdownAllUsersAsync(int? month = null, int? year = null);
}
