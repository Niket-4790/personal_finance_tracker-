using PersonalFinanceTracker.Models;

namespace PersonalFinanceTracker.Services;

public interface IReportPdfService
{
    byte[] GenerateReportPdf(
        List<MonthlyReport> monthlyReports,
        List<CategoryBreakdown> categoryBreakdown,
        decimal savingsRate,
        decimal avgExpense,
        MonthlyReport bestMonth,
        bool isAdmin);
}
