using PersonalFinanceTracker.Models;

namespace PersonalFinanceTracker.Services;

public interface IAccountService
{
    Task<IEnumerable<Account>> GetAllAccountsAsync(int userId);
    Task<Account?> GetAccountByIdAsync(int id, int userId);
    Task<int> CreateAccountAsync(AccountFormModel model, int userId);
    Task UpdateAccountAsync(AccountFormModel model, int userId);
    Task DeleteAccountAsync(int id, int userId);
}

public interface ICategoryService
{
    Task<IEnumerable<Category>> GetAllCategoriesAsync(int userId);
    Task<IEnumerable<Category>> GetCategoriesByTypeAsync(int userId, string type);
    Task<int> CreateCategoryAsync(Category category, int userId);
}

public interface ITransactionService
{
    Task<IEnumerable<Transaction>> GetAllTransactionsAsync(int userId);
    Task<Transaction?> GetTransactionByIdAsync(int id, int userId);
    Task<int> CreateTransactionAsync(TransactionFormModel model, int userId);
    Task UpdateTransactionAsync(TransactionFormModel model, int userId);
    Task DeleteTransactionAsync(int id, int userId);

    /// <summary>Admin-only: every transaction across every user, read-only.</summary>
    Task<IEnumerable<Transaction>> GetAllTransactionsForAdminAsync();
}

public interface IReportService
{
    Task<DashboardSummary> GetDashboardSummaryAsync(int userId);
    Task<IEnumerable<MonthlyReport>> GetMonthlyReportsAsync(int userId, int monthsBack = 6);
    Task<IEnumerable<CategoryBreakdown>> GetCategoryBreakdownAsync(int userId, int? month = null, int? year = null);
    Task<decimal> GetSavingsRateAsync(int userId, int? month = null, int? year = null);

    // Admin-only: aggregate across every user.
    Task<SystemSummary> GetSystemSummaryAsync();
    Task<IEnumerable<MonthlyReport>> GetMonthlyReportsAllUsersAsync(int monthsBack = 6);
    Task<IEnumerable<CategoryBreakdown>> GetCategoryBreakdownAllUsersAsync(int? month = null, int? year = null);
}

/// <summary>
/// Answers "is this login valid?" / "create this new account" for the
/// anonymous-facing Login/Register pages. Self-service registration only ever
/// creates a 'User' account - kept separate from IUserService so the
/// admin-power methods there are never reachable from a public-facing form.
/// </summary>
public interface IAuthService
{
    Task RegisterAsync(RegisterModel model);
    Task<AppUser?> ValidateCredentialsAsync(string email, string password);
}

/// <summary>
/// Answers "list every user, deactivate this one, promote that one to Admin" -
/// used only by the Admin-only Users page.
/// </summary>
public interface IUserService
{
    Task<IEnumerable<AppUser>> GetAllUsersAsync();
    Task SetRoleAsync(int id, string role);
    Task SetActiveAsync(int id, bool isActive);
}
