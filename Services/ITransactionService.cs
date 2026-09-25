
using PersonalFinanceTracker.Models;

namespace PersonalFinanceTracker.Services;

public interface ITransactionService
{
    Task<IEnumerable<Transaction>> GetAllTransactionsAsync(
        int userId);

    Task<Transaction?> GetTransactionByIdAsync(
        int id,
        int userId);

    Task<int> CreateTransactionAsync(
        TransactionFormModel model,
        int userId);

    Task UpdateTransactionAsync(
        TransactionFormModel model,
        int userId);

    Task DeleteTransactionAsync(
        int id,
        int userId);

    /// <summary>
    /// Admin-only: every transaction across every user, read-only.
    /// </summary>
    Task<IEnumerable<Transaction>> GetAllTransactionsForAdminAsync();
}

