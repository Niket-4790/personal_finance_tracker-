using PersonalFinanceTracker.Models;

namespace PersonalFinanceTracker.Services;

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
    Task<AppUser?> AuthenticateGoogleAsync(string googleSubjectId, string name,string email);
}
