namespace PersonalFinanceTracker.Models;

/// <summary>
/// Internal shape for IUserRepository.GetByEmailWithHashAsync.
///
/// Carries the PasswordHash so AuthService can verify it.
/// </summary>
public class UserCredential
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? PasswordHash { get; set; }

    public string Role { get; set; } = "User";

    public bool IsActive { get; set; } = true;

    public string? GoogleSubjectId { get; set; }
}