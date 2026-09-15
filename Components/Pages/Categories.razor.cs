using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using PersonalFinanceTracker.Models;
using System.Security.Claims;

namespace PersonalFinanceTracker.Components.Pages
{
    public partial class Categories
    {
        [CascadingParameter]
        private Task<AuthenticationState>? AuthStateTask { get; set; }

        private int currentUserId;

        private bool showForm;
        private string? message;
        private string? error;
        private Category newCategory = new() { Type = "Expense", Color = "#6366f1" };
        private List<Category> categories = [];

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
            error = null;
            try
            {
                categories = (await CategoryService.GetAllCategoriesAsync(currentUserId)).ToList();
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }
        }

        private async Task SaveCategory()
        {
            try
            {
                await CategoryService.CreateCategoryAsync(newCategory, currentUserId);
                message = "Category created successfully.";
                showForm = false;
                newCategory = new Category { Type = "Expense", Color = "#6366f1" };
                await LoadData();
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }
        }
    }
}
