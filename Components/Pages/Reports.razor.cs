using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using PersonalFinanceTracker.Models;
using System.Security.Claims;
using PersonalFinanceTracker.Services;

namespace PersonalFinanceTracker.Components.Pages
{
    public partial class Reports
    {

        [Inject]
        private IJSRuntime JS { get; set; } = default!;

        [Inject]
        private IPaymentService PaymentService { get; set; } = default!;

        [CascadingParameter]
        private Task<AuthenticationState>? AuthStateTask { get; set; }

        private bool isAdmin;

        private int currentUserId;

        private bool loading = true;

        private string? error;
        private decimal savingsRate;
        private decimal avgExpense;
        private MonthlyReport bestMonth = new();
        private List<MonthlyReport> monthlyReports = [];
        private List<CategoryBreakdown> categoryBreakdown = [];

        private async Task DownloadPdf()
        {
            if (!isAdmin)
            {
                var isPremium =
    await PaymentService.IsPremiumAsync(currentUserId);

                if (!isPremium)
                {
                    error =
                        "PDF reports are available only on the Premium plan. Upgrade to Premium to download reports.";

                    return;
                }
            }

            var pdf = ReportPdfService.GenerateReportPdf(
                monthlyReports,
                categoryBreakdown,
                savingsRate,
                avgExpense,
                bestMonth,
                isAdmin);

            await JS.InvokeVoidAsync(
                "downloadFile",
                "FinanceTrack_Report.pdf",
                "application/pdf",
                pdf);
        }

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
                    var userIdClaim = authState.User.FindFirst("AppUserId")?.Value;

                    if (!int.TryParse(userIdClaim, out currentUserId))
                    {
                        throw new InvalidOperationException(
                            "Application user ID is missing or invalid.");
                    }
                    monthlyReports = (await ReportService.GetMonthlyReportsAsync(currentUserId, 6)).ToList();
                    categoryBreakdown = (await ReportService.GetCategoryBreakdownAsync(currentUserId)).ToList();
                    savingsRate = await ReportService.GetSavingsRateAsync(currentUserId);
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
