using Microsoft.AspNetCore.Identity;
using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.Repositories;

namespace PersonalFinanceTracker.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly PasswordHasher<UserCredential> _passwordHasher = new();

    public AuthService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task RegisterAsync(RegisterModel model)
    {
        var existing = await _userRepository.GetByEmailWithHashAsync(model.Email);
        if (existing is not null)
            throw new InvalidOperationException("A user with that email already exists.");

        var temp = new UserCredential
        {
            Name = model.Name,
            Email = model.Email
        };

        var hash = _passwordHasher.HashPassword(temp, model.Password);

        await _userRepository.CreateAsync(model.Name.Trim(), model.Email.Trim(), hash, "User");
    }

    public async Task<AppUser?> ValidateCredentialsAsync(string email, string password)
    {
        var cred = await _userRepository.GetByEmailWithHashAsync(email);
        if (cred is null || cred.PasswordHash is null)
            return null;

        var result = _passwordHasher.VerifyHashedPassword(cred, cred.PasswordHash, password);
        if (result == PasswordVerificationResult.Failed)
            return null;

        if (!cred.IsActive)
            return null;

        // Map to AppUser
        return new AppUser
        {
            Id = cred.Id,
            Name = cred.Name,
            Email = cred.Email,
            Role = cred.Role,
            IsActive = cred.IsActive,
            GoogleSubjectId = cred.GoogleSubjectId,
            CreatedAt = DateTime.UtcNow // repository returns CreatedAt normally; set placeholder if needed
        };
    }

    public async Task<AppUser?> AuthenticateGoogleAsync(string googleSubjectId, string name, string email)
    {
        // Try to find by Google subject id
        var user = await _userRepository.GetByGoogleSubjectIdAsync(googleSubjectId);
        if (user is not null)
            return user.IsActive ? user : null;

        // Try to find by email - if exists, attach Google subject id
        var byEmail = await _userRepository.GetByEmailWithHashAsync(email);
        if (byEmail is not null)
        {
            var appUser = await _userRepository.GetByIdAsync(byEmail.Id);
            if (appUser is null || !appUser.IsActive)
                return null;

            await _userRepository.SetGoogleSubjectIdAsync(appUser.Id, googleSubjectId);
            appUser.GoogleSubjectId = googleSubjectId;
            return appUser;
        }

        // Create a new Google user
        var newId = await _userRepository.CreateGoogleUserAsync(name.Trim(), email.Trim(), googleSubjectId);
        var newUser = await _userRepository.GetByIdAsync(newId);
        if (newUser is null)
            return null;

        return newUser.IsActive ? newUser : null;
    }
}
