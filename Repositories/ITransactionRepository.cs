using PersonalFinanceTracker.Models;

namespace PersonalFinanceTracker.Repositories;

public interface ITransactionRepository
{
    Task<IEnumerable<Transaction>> GetAllAsync(int userId);
    Task<Transaction?> GetByIdAsync(int id, int userId);
    Task<int> InsertAsync(Transaction transaction);
    Task UpdateAsync(Transaction transaction);
    Task DeleteAsync(int id, int userId);

    /// <summary>Admin-only: every transaction across every user.</summary>
    Task<IEnumerable<Transaction>> GetAllForAdminAsync();
}
