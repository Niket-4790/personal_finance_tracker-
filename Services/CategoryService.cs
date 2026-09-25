using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.Repositories;

namespace PersonalFinanceTracker.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public Task<IEnumerable<Category>> GetAllCategoriesAsync(int userId) =>
        _categoryRepository.GetAllAsync(userId);

    public async Task<IEnumerable<Category>> GetCategoriesByTypeAsync(int userId, string type)
    {
        var categories = await _categoryRepository.GetAllAsync(userId);
        return categories.Where(c => c.Type.Equals(type, StringComparison.OrdinalIgnoreCase));
    }

    public Task<int> CreateCategoryAsync(Category category, int userId)
    {
        category.UserId = userId;
        category.Name = category.Name.Trim();
        return _categoryRepository.InsertAsync(category);
    }
}
