using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.Repositories;

namespace PersonalFinanceTracker.Services;

public class ReportService : IReportService
{
    private readonly IReportRepository _reportRepository;

    public ReportService(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public async Task<DashboardSummary> GetDashboardSummaryAsync(int userId)
    {
        var summary = await _reportRepository.GetDashboardSummaryAsync(userId);
        return summary ?? new DashboardSummary();
    }

    public Task<IEnumerable<MonthlyReport>> GetMonthlyReportsAsync(int userId, int monthsBack = 6) =>
        _reportRepository.GetMonthlyTotalsAsync(userId, monthsBack);

    public Task<IEnumerable<CategoryBreakdown>> GetCategoryBreakdownAsync(int userId, int? month = null, int? year = null) =>
        _reportRepository.GetCategoryBreakdownAsync(userId, month, year);

    public Task<decimal> GetSavingsRateAsync(int userId, int? month = null, int? year = null) =>
        _reportRepository.GetSavingsRateAsync(userId, month, year);

    public async Task<SystemSummary> GetSystemSummaryAsync()
    {
        var summary = await _reportRepository.GetSystemSummaryAsync();
        return summary ?? new SystemSummary();
    }

    public Task<IEnumerable<MonthlyReport>> GetMonthlyReportsAllUsersAsync(int monthsBack = 6) =>
        _reportRepository.GetMonthlyTotalsAllUsersAsync(monthsBack);

    public Task<IEnumerable<CategoryBreakdown>> GetCategoryBreakdownAllUsersAsync(int? month = null, int? year = null) =>
        _reportRepository.GetCategoryBreakdownAllUsersAsync(month, year);
}
