using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using PersonalFinanceTracker.Models;
using System.Security.Claims;
using AccountModel = PersonalFinanceTracker.Models.Account;

namespace PersonalFinanceTracker.Components.Pages
{
    public partial class Transactions
    {

        [CascadingParameter]
        private Task<AuthenticationState>? AuthStateTask { get; set; }

        private int currentUserId;
        private bool isAdmin;

        private bool loading = true;
        private bool showForm;
        private int? editingId;
        private string? message;
        private string? error;
        private TransactionFormModel formModel = new();
        private List<Transaction> transactions = [];
        private List<AccountModel> accounts = [];
        private List<Category> categories = [];

        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthStateTask!;
            isAdmin = authState.User.IsInRole("Admin");
            if (!isAdmin)
            {
                currentUserId = int.Parse(authState.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            }

            await LoadData();
        }

        private async Task LoadData()
        {
            loading = true;
            error = null;
            try
            {
                if (isAdmin)
                {
                    transactions = (await TransactionService.GetAllTransactionsForAdminAsync()).ToList();
                }
                else
                {
                    transactions = (await TransactionService.GetAllTransactionsAsync(currentUserId)).ToList();
                    accounts = (await AccountService.GetAllAccountsAsync(currentUserId)).ToList();
                    categories = (await CategoryService.GetAllCategoriesAsync(currentUserId)).ToList();
                }
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
            formModel = new TransactionFormModel { TransactionDate = DateTime.Today };
            showForm = true;
            message = null;
        }

        private void EditTransaction(Transaction tx)
        {
            editingId = tx.Id;
            formModel = new TransactionFormModel
            {
                Id = tx.Id,
                AccountId = tx.AccountId,
                CategoryId = tx.CategoryId,
                Amount = tx.Amount,
                Description = tx.Description ?? string.Empty,
                TransactionDate = tx.TransactionDate,
                TransactionType = tx.TransactionType
            };
            showForm = true;
            message = null;
        }

        private async Task SaveTransaction(TransactionFormModel model)
        {
            if (editingId.HasValue)
            {
                model.Id = editingId.Value;
                await TransactionService.UpdateTransactionAsync(model, currentUserId);
                message = "Transaction updated successfully.";
            }
            else
            {
                await TransactionService.CreateTransactionAsync(model, currentUserId);
                message = "Transaction created successfully.";
            }

            showForm = false;
            await LoadData();
        }

        private void CancelForm() => showForm = false;

        private async Task DeleteTransaction(int id)
        {
            try
            {
                await TransactionService.DeleteTransactionAsync(id, currentUserId);
                message = "Transaction deleted.";
                await LoadData();
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }
        }
    }
}
