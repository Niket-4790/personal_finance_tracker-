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
            currentUserId = int.Parse(authState.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
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
