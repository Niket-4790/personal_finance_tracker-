using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.Repositories;

namespace PersonalFinanceTracker.Services;

public class BudgetService : IBudgetService
{
    private readonly IBudgetRepository _budgetRepository;
    private readonly IPaymentService _paymentService;

    public BudgetService(
        IBudgetRepository budgetRepository,
        IPaymentService paymentService)
    {
        _budgetRepository = budgetRepository;
        _paymentService = paymentService;
    }

    public Task<IEnumerable<Budget>> GetAllBudgetsAsync(
        int userId,
        DateTime month) =>
        _budgetRepository.GetAllAsync(userId, month);

    public Task<Budget?> GetBudgetByIdAsync(
        int id,
        int userId) =>
        _budgetRepository.GetByIdAsync(id, userId);

    public async Task<int> CreateBudgetAsync(
        Budget budget,
        int userId)
    {
        ValidateBudget(budget);

        var isPremium =await _paymentService.IsPremiumAsync(userId);

        if (!isPremium)
        {
            var existingBudgets =
                await _budgetRepository.GetAllAsync(
                    userId,
                    budget.BudgetMonth);

            if (existingBudgets.Any())
            {
                throw new InvalidOperationException(
    "Free plan allows only 1 budget per month. Upgrade to Premium to create multiple budgets per month.");
            }
        }

        budget.UserId = userId;

        budget.BudgetMonth = new DateTime(
            budget.BudgetMonth.Year,
            budget.BudgetMonth.Month,
            1);

        return await _budgetRepository.InsertAsync(budget);
    }

    public async Task UpdateBudgetAsync(
        Budget budget,
        int userId)
    {
        ValidateBudget(budget);

        var existing = await _budgetRepository.GetByIdAsync(
            budget.Id,
            userId);

        if (existing is null)
            throw new InvalidOperationException(
                $"Budget {budget.Id} not found.");

        budget.UserId = userId;

        budget.BudgetMonth = new DateTime(
            budget.BudgetMonth.Year,
            budget.BudgetMonth.Month,
            1);

        await _budgetRepository.UpdateAsync(budget);
    }

    public Task DeleteBudgetAsync(
        int id,
        int userId) =>
        _budgetRepository.DeleteAsync(id, userId);

    private static void ValidateBudget(Budget budget)
    {
        if (budget.CategoryId <= 0)
            throw new ArgumentException(
                "Please select a category.");

        if (budget.Amount <= 0)
            throw new ArgumentException(
                "Budget amount must be greater than zero.");

        if (budget.BudgetMonth == default)
            throw new ArgumentException(
                "Please select a budget month.");
    }
}
