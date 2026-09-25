using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.Repositories;

namespace PersonalFinanceTracker.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public Task<IEnumerable<AppUser>> GetAllUsersAsync() =>
        _userRepository.GetAllAsync();

    public Task SetRoleAsync(int id, string role) =>
        _userRepository.SetRoleAsync(id, role);

    public Task SetActiveAsync(int id, bool isActive) =>
        _userRepository.SetActiveAsync(id, isActive);
}
