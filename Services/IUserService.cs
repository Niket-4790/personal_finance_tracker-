using PersonalFinanceTracker.Models;

namespace PersonalFinanceTracker.Services;

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
