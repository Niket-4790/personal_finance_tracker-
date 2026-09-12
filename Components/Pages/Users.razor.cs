using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using PersonalFinanceTracker.Models;
using System.Security.Claims;

namespace PersonalFinanceTracker.Components.Pages
{
    public partial class Users
    {
        [CascadingParameter]
        private Task<AuthenticationState>? AuthStateTask { get; set; }

        private int currentUserId;
        private bool loading = true;
        private string? message;
        private string? error;
        private List<AppUser> users = [];

        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthStateTask!;
            currentUserId = int.Parse(authState.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await LoadData();
        }

        private async Task LoadData()
        {
            loading = true;
            error = null;
            try
            {
                users = (await UserService.GetAllUsersAsync()).ToList();
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }
            finally
            {
                loading = false;
            }
        }

        private async Task ChangeRole(AppUser user, string newRole)
        {
            try
            {
                await UserService.SetRoleAsync(user.Id, newRole);
                message = $"{user.Name}'s role updated to {newRole}.";
                await LoadData();
            }
            catch (Exception ex)
            {
                error = ex.Message;
                await LoadData();
            }
        }

        private async Task ToggleActive(AppUser user)
        {
            try
            {
                await UserService.SetActiveAsync(user.Id, !user.IsActive);
                message = $"{user.Name} {(user.IsActive ? "deactivated" : "activated")}.";
                await LoadData();
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }
        }
    }
}
