using PersonalFinanceTracker.Models;

namespace PersonalFinanceTracker.Repositories;

public interface IAccountRepository
{
    Task<IEnumerable<Account>> GetAllAsync(int userId);
    Task<Account?> GetByIdAsync(int id, int userId);
    Task<int> InsertAsync(Account account);
    Task UpdateAsync(Account account);
    Task DeleteAsync(int id, int userId);
}
