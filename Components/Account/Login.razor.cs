using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using PersonalFinanceTracker.Models;

namespace PersonalFinanceTracker.Components.Account;

public partial class Login
{
    [SupplyParameterFromForm]
    private LoginModel Input { get; set; } = new();

    [SupplyParameterFromQuery(Name = "error")]
    private string? Error { get; set; }

    [CascadingParameter]
    //A cascading parameter in Blazor is a way for a parent component to provide a
    //value to its child components without passing it manually through every component
    private HttpContext HttpContext { get; set; } = default!;

    private string? error;

    protected override void OnInitialized()
    {
        if (Error == "inactive")
        {
            error = "Your account is inactive.";
        }
    }

    private async Task LoginUser()
    {
        var user = await AuthService.ValidateCredentialsAsync(
            Input.Email,
            Input.Password);

        if (user is null)
        {
            error = "Invalid email or password, or the account is inactive.";
            return;
        }

        var claims = new List<Claim>
        {
            new("AppUserId", user.Id.ToString()),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        NavigationManager.NavigateTo("/", forceLoad: true);
    }
}