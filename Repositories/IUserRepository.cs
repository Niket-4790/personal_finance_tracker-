using PersonalFinanceTracker.Models;

namespace PersonalFinanceTracker.Repositories;

public interface IUserRepository
{
    Task<int> CreateAsync(string name, string email, string passwordHash, string role);
    Task<UserCredential?> GetByEmailWithHashAsync(string email);

    Task<AppUser?> GetByGoogleSubjectIdAsync(
        string googleSubjectId);

    Task<int> CreateGoogleUserAsync(
        string name,
        string email,
        string googleSubjectId);

    Task SetGoogleSubjectIdAsync(
        int userId,
        string googleSubjectId);
    Task<AppUser?> GetByIdAsync(int id);
    Task<IEnumerable<AppUser>> GetAllAsync();
    Task SetRoleAsync(int id, string role);
    Task SetActiveAsync(int id, bool isActive);
    Task<int> GetAdminCountAsync();
    Task<int> GetActiveAdminCountAsync();
}
