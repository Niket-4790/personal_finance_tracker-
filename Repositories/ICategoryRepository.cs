using PersonalFinanceTracker.Models;

namespace PersonalFinanceTracker.Repositories;

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetAllAsync(int userId);
    Task<Category?> GetByIdAsync(int id, int userId);
    Task<int> InsertAsync(Category category);
}
