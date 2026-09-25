using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.Repositories;

namespace PersonalFinanceTracker.Services;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IBudgetService _budgetService;
    private readonly INotificationService _notificationService;

    public TransactionService(
        ITransactionRepository transactionRepository,
        IBudgetService budgetService,
        INotificationService notificationService)
    {
        _transactionRepository = transactionRepository;
        _budgetService = budgetService;
        _notificationService = notificationService;
    }

    public Task<IEnumerable<Transaction>> GetAllTransactionsAsync(int userId) =>
        _transactionRepository.GetAllAsync(userId);

    public Task<Transaction?> GetTransactionByIdAsync(int id, int userId) =>
        _transactionRepository.GetByIdAsync(id, userId);

    public async Task<int> CreateTransactionAsync(
        TransactionFormModel model,
        int userId)
    {
        ValidateTransaction(model);

        var transaction = MapToEntity(model, userId);

        // Get budget before adding the expense.
        Budget? budget = null;

        if (model.TransactionType == "Expense")
        {
            var budgets =
                await _budgetService.GetAllBudgetsAsync(
                    userId,
                    new DateTime(
                        model.TransactionDate.Year,
                        model.TransactionDate.Month,
                        1));

            budget = budgets.FirstOrDefault(
                b => b.CategoryId == model.CategoryId);
        }

        var transactionId =
            await _transactionRepository.InsertAsync(transaction);

        // Check budget after adding the expense.
        if (budget is not null)
        {
            var updatedBudgets =
                await _budgetService.GetAllBudgetsAsync(
                    userId,
                    new DateTime(
                        model.TransactionDate.Year,
                        model.TransactionDate.Month,
                        1));

            var updatedBudget =
                updatedBudgets.FirstOrDefault(
                    b => b.CategoryId == model.CategoryId);

            if (updatedBudget is not null)
            {
                if (budget.PercentageUsed < 80 &&
                    updatedBudget.PercentageUsed >= 80 &&
                    updatedBudget.PercentageUsed < 100)
                {
                    await _notificationService.CreateNotificationAsync(
                        userId,
                        "Budget Warning",
                        $"You have used 80% of your {updatedBudget.CategoryName} budget.");
                }

                if (budget.PercentageUsed < 100 &&
                    updatedBudget.PercentageUsed >= 100)
                {
                    await _notificationService.CreateNotificationAsync(
                        userId,
                        "Budget Exceeded",
                        $"You have exceeded your {updatedBudget.CategoryName} budget.",true);
                }
            }
        }

        return transactionId;
    }

    public async Task UpdateTransactionAsync(
        TransactionFormModel model,
        int userId)
    {
        ValidateTransaction(model);

        var existing =
            await _transactionRepository.GetByIdAsync(
                model.Id,
                userId)
            ?? throw new InvalidOperationException(
                $"Transaction {model.Id} not found.");

        var transaction = MapToEntity(model, userId);
        transaction.Id = existing.Id;

        await _transactionRepository.UpdateAsync(transaction);
    }

    public Task DeleteTransactionAsync(int id, int userId) =>
        _transactionRepository.DeleteAsync(id, userId);

    public Task<IEnumerable<Transaction>> GetAllTransactionsForAdminAsync() =>
        _transactionRepository.GetAllForAdminAsync();

    private static void ValidateTransaction(
        TransactionFormModel model)
    {
        if (model.AccountId <= 0)
            throw new ArgumentException(
                "Please select an account.");

        if (model.CategoryId <= 0)
            throw new ArgumentException(
                "Please select a category.");

        if (model.Amount <= 0)
            throw new ArgumentException(
                "Amount must be greater than zero.");
    }

    private static Transaction MapToEntity(
        TransactionFormModel model,
        int userId) => new()
        {
            Id = model.Id,
            UserId = userId,
            AccountId = model.AccountId,
            CategoryId = model.CategoryId,
            Amount = model.Amount,
            Description =
                string.IsNullOrWhiteSpace(model.Description)
                    ? null
                    : model.Description.Trim(),
            TransactionDate = model.TransactionDate.Date,
            TransactionType = model.TransactionType
        };
}