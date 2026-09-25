using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using PersonalFinanceTracker.Models;
using System.Globalization;

namespace PersonalFinanceTracker.Components.Pages;

public partial class Budgets
{
    [CascadingParameter]
    private Task<AuthenticationState>? AuthStateTask { get; set; }

    private int currentUserId;

    private bool loading = true;
    private bool showForm;
    private int? editingId;

    private string? message;
    private string? error;

    private DateTime selectedMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

    private int selectedCategoryId;
    private decimal budgetAmount;

    private List<Budget> budgets = [];
    private List<Category> expenseCategories = [];

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthStateTask!;

        var userIdClaim = authState.User.FindFirst("AppUserId")?.Value;

        if (!int.TryParse(userIdClaim, out currentUserId))
        {
            throw new InvalidOperationException(
                "Application user ID is missing or invalid.");
        }

        await LoadData();
    }

    private async Task LoadData()
    {
        loading = true;
        error = null;

        try
        {
            budgets = (
                await BudgetService.GetAllBudgetsAsync(
                    currentUserId,
                    selectedMonth)
            ).ToList();

            expenseCategories = (
                await CategoryService.GetCategoriesByTypeAsync(
                    currentUserId,
                    "Expense")
            ).ToList();
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

    private async Task ChangeMonth(ChangeEventArgs e)
    {
        if (DateTime.TryParseExact(
            e.Value?.ToString(),
            "yyyy-MM",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var month))
        {
            selectedMonth = new DateTime(
                month.Year,
                month.Month,
                1);

            showForm = false;
            message = null;

            await LoadData();
        }
    }

    private void ShowAddForm()
    {
        editingId = null;

        selectedCategoryId = 0;
        budgetAmount = 0;

        showForm = true;
        message = null;
        error = null;
    }

    private void EditBudget(Budget budget)
    {
        editingId = budget.Id;

        selectedCategoryId = budget.CategoryId;
        budgetAmount = budget.Amount;

        selectedMonth = new DateTime(
            budget.BudgetMonth.Year,
            budget.BudgetMonth.Month,
            1);

        showForm = true;
        message = null;
        error = null;
    }

    private async Task SaveBudget()
    {
        try
        {
            error = null;

            if (selectedCategoryId <= 0)
            {
                error = "Please select a category.";
                return;
            }

            if (budgetAmount <= 0)
            {
                error = "Budget amount must be greater than zero.";
                return;
            }

            var budget = new Budget
            {
                Id = editingId ?? 0,
                UserId = currentUserId,
                CategoryId = selectedCategoryId,
                BudgetMonth = selectedMonth,
                Amount = budgetAmount
            };

            if (editingId.HasValue)
            {
                await BudgetService.UpdateBudgetAsync(
                    budget,
                    currentUserId);

                message = "Budget updated successfully.";
            }
            else
            {
                await BudgetService.CreateBudgetAsync(
                    budget,
                    currentUserId);

                message = "Budget created successfully.";
            }

            showForm = false;

            await LoadData();
        }
        catch (Exception ex)
        {
            error = ex.Message;
        }
    }

    private void CancelForm()
    {
        showForm = false;
        error = null;
    }

    private async Task DeleteBudget(int id)
    {
        try
        {
            error = null;

            await BudgetService.DeleteBudgetAsync(
                id,
                currentUserId);

            message = "Budget deleted successfully.";

            await LoadData();
        }
        catch (Exception ex)
        {
            error = ex.Message;
        }
    }

    private static string GetProgressWidth(decimal percentage)
    {
        var width = Math.Clamp(percentage, 0, 100);

        return $"{width:0.##}%";
    }

    private static string GetProgressColor(decimal percentage)
    {
        if (percentage >= 100)
            return "var(--expense)";

        if (percentage >= 80)
            return "var(--warning)";

        return "var(--income)";
    }
}