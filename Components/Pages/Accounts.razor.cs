using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using PersonalFinanceTracker.Models;
using System.Security.Claims;
using AccountModel = PersonalFinanceTracker.Models.Account;

namespace PersonalFinanceTracker.Components.Pages
{
    public partial class Accounts
    {
        [CascadingParameter]
        private Task<AuthenticationState>? AuthStateTask { get; set; }

        private int currentUserId;

        private bool loading = true;
        private bool showForm;
        private int? editingId;
        private string? message;
        private string? error;
        private AccountFormModel formModel = new();
        private List<AccountModel> accounts = [];

        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthStateTask!;
           var userIdClaim = authState.User.FindFirst("AppUserId")?.Value;

if (!int.TryParse(userIdClaim, out currentUserId))
{
    throw new InvalidOperationException(
        "Application user ID is missing or invalid.");
}
            await LoadData();
        }

        private async Task LoadData()
        {
            loading = true;
            error = null;
            try
            {
                accounts = (await AccountService.GetAllAccountsAsync(currentUserId)).ToList();
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

        private void ShowAddForm()
        {
            editingId = null;
            formModel = new AccountFormModel();
            showForm = true;
            message = null;
        }

        private void EditAccount(AccountModel account)
        {
            editingId = account.Id;
            formModel = new AccountFormModel
            {
                Id = account.Id,
                Name = account.Name,
                AccountType = account.AccountType
            };
            showForm = true;
            message = null;
        }

        private async Task SaveAccount()
        {
            try
            {
                error = null;
                message = null;

                if (editingId.HasValue)
                {
                    formModel.Id = editingId.Value;
                    await AccountService.UpdateAccountAsync(formModel, currentUserId);
                    message = "Account updated successfully.";
                }
                else
                {
                    await AccountService.CreateAccountAsync(formModel, currentUserId);
                    message = "Account created successfully.";
                }

                showForm = false;
                await LoadData();
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }
        }

        private void CancelForm() => showForm = false;

        private async Task DeleteAccount(int id)
        {
            try
            {
                await AccountService.DeleteAccountAsync(id, currentUserId);
                message = "Account deleted.";
                await LoadData();
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }
        }
    }
}
