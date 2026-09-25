using PersonalFinanceTracker.Models;

namespace PersonalFinanceTracker.Repositories;

public interface IBudgetRepository
{
    Task<IEnumerable<Budget>> GetAllAsync(int userId, DateTime month);

    Task<Budget?> GetByIdAsync(int id, int userId);

    Task<int> InsertAsync(Budget budget);

    Task UpdateAsync(Budget budget);

    Task DeleteAsync(int id, int userId);
}