using PersonalFinanceTracker.Models;

namespace PersonalFinanceTracker.Services;

public interface IBudgetService
{
    Task<IEnumerable<Budget>> GetAllBudgetsAsync(
        int userId,
        DateTime month);

    Task<Budget?> GetBudgetByIdAsync(
        int id,
        int userId);

    Task<int> CreateBudgetAsync(
        Budget budget,
        int userId);

    Task UpdateBudgetAsync(
        Budget budget,
        int userId);

    Task DeleteBudgetAsync(
        int id,
        int userId);
}
