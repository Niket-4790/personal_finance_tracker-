using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using PersonalFinanceTracker.Models;
using System.Security.Claims;

namespace PersonalFinanceTracker.Components.Pages
{
    public partial class Reports
    {
        [CascadingParameter]
        private Task<AuthenticationState>? AuthStateTask { get; set; }

        private bool isAdmin;

        private bool loading = true;
        private string? error;
        private decimal savingsRate;
        private decimal avgExpense;
        private MonthlyReport bestMonth = new();
        private List<MonthlyReport> monthlyReports = [];
        private List<CategoryBreakdown> categoryBreakdown = [];

        protected override async Task OnInitializedAsync()
        {
            try
            {
                var authState = await AuthStateTask!;
                isAdmin = authState.User.IsInRole("Admin");

                if (isAdmin)
                {
                    monthlyReports = (await ReportService.GetMonthlyReportsAllUsersAsync(6)).ToList();
                    categoryBreakdown = (await ReportService.GetCategoryBreakdownAllUsersAsync()).ToList();
                }
                else
                {
                    var userId = int.Parse(authState.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                    monthlyReports = (await ReportService.GetMonthlyReportsAsync(userId, 6)).ToList();
                    categoryBreakdown = (await ReportService.GetCategoryBreakdownAsync(userId)).ToList();
                    savingsRate = await ReportService.GetSavingsRateAsync(userId);
                }

                avgExpense = monthlyReports.Any() ? monthlyReports.Average(r => r.TotalExpenses) : 0;
                bestMonth = monthlyReports.OrderByDescending(r => r.NetSavings).FirstOrDefault() ?? new MonthlyReport();
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }
            finally
            {
                loading = false;
            }
        }
    }
}
