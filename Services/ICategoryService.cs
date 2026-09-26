
using PersonalFinanceTracker.Models;

namespace PersonalFinanceTracker.Services;

public interface ICategoryService
{
    Task<IEnumerable<Category>> GetAllCategoriesAsync(int userId);

    Task<IEnumerable<Category>> GetCategoriesByTypeAsync( int userId, string type);

    Task<int> CreateCategoryAsync( Category category,int userId);
}

