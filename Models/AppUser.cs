namespace PersonalFinanceTracker.Models;

/// <summary>
/// A user account - Admin manages the system, User owns their own
/// financial data.
///
/// Never carries the password hash. The password hash is handled
/// separately through UserCredential.
/// </summary>
public class AppUser
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = "User";

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public string? GoogleSubjectId { get; set; }
}