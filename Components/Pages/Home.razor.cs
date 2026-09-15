using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using PersonalFinanceTracker.Models;
using System.Security.Claims;

namespace PersonalFinanceTracker.Components.Pages
{
    public partial class Home
    {
        [CascadingParameter]
        private Task<AuthenticationState>? AuthStateTask { get; set; }

        private bool loading = true;
        private bool isAdmin;
        private string? error;

        private DashboardSummary summary = new();
        private decimal savingsRate;

        private SystemSummary systemSummary = new();

        private List<MonthlyReport> monthlyReports = [];
        private List<CategoryBreakdown> categoryBreakdown = [];
        private List<Transaction> recentTransactions = [];

        protected override async Task OnInitializedAsync()
        {
            try
            {
                var authState = await AuthStateTask!;
                isAdmin = authState.User.IsInRole("Admin");

                if (isAdmin)
                {
                    systemSummary = await ReportService.GetSystemSummaryAsync();
                    monthlyReports = (await ReportService.GetMonthlyReportsAllUsersAsync(6)).ToList();
                    categoryBreakdown = (await ReportService.GetCategoryBreakdownAllUsersAsync()).ToList();
                    recentTransactions = (await TransactionService.GetAllTransactionsForAdminAsync()).Take(8).ToList();
                }
                else
                {
                    var userIdClaim = authState.User.FindFirst("AppUserId")?.Value;

                    if (!int.TryParse(userIdClaim, out var userId))
                    {
                        throw new InvalidOperationException(
                            "Application user ID is missing or invalid.");
                    }

                    summary = await ReportService.GetDashboardSummaryAsync(userId);

                    savingsRate =
                        await ReportService.GetSavingsRateAsync(userId);

                    monthlyReports =
                        (await ReportService.GetMonthlyReportsAsync(userId, 6))
                        .ToList();

                    categoryBreakdown =
                        (await ReportService.GetCategoryBreakdownAsync(userId))
                        .ToList();

                    recentTransactions =
                        (await TransactionService.GetAllTransactionsAsync(userId))
                        .Take(8)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                error = $"Could not load dashboard. Ensure SQL Server is running and database scripts have been executed. Details: {ex.Message}";
            }
            finally
            {
                loading = false;
            }
        }
    }
}
