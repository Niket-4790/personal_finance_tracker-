using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using PersonalFinanceTracker.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace PersonalFinanceTracker.Components.Account
{
    public partial class Register
    {
        [SupplyParameterFromForm]
        private RegisterModel Input { get; set; } = new();

        [CascadingParameter]
        private HttpContext HttpContext { get; set; } = default!;

        private string? error;

        private async Task RegisterUser()
        {
            try
            {
                await AuthService.RegisterAsync(Input);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return;
            }

            // Sign the new account straight in rather than bouncing to a second
            // login form - one less step for a brand-new user.
            var user = await AuthService.ValidateCredentialsAsync(Input.Email, Input.Password)
                ?? throw new InvalidOperationException("Registration succeeded but sign-in failed unexpectedly.");

            var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            NavigationManager.NavigateTo("/", forceLoad: true);
        }
    }
}
