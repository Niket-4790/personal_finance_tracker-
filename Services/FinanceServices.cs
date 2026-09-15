using Microsoft.AspNetCore.Identity;
using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.Repositories;

namespace PersonalFinanceTracker.Services;

public class AccountService : IAccountService
{
    private readonly IAccountRepository _accountRepository;

    public AccountService(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public Task<IEnumerable<Account>> GetAllAccountsAsync(int userId) =>
        _accountRepository.GetAllAsync(userId);

    public Task<Account?> GetAccountByIdAsync(int id, int userId) =>
        _accountRepository.GetByIdAsync(id, userId);

    public Task<int> CreateAccountAsync(AccountFormModel model, int userId)
    {
        var account = new Account
        {
            UserId = userId,
            Name = model.Name.Trim(),
            AccountType = model.AccountType,
            Balance = model.InitialBalance
        };
        return _accountRepository.InsertAsync(account);
    }

    public async Task UpdateAccountAsync(AccountFormModel model, int userId)
    {
        var existing = await _accountRepository.GetByIdAsync(model.Id, userId)
            ?? throw new InvalidOperationException($"Account {model.Id} not found.");

        existing.Name = model.Name.Trim();
        existing.AccountType = model.AccountType;
        await _accountRepository.UpdateAsync(existing);
    }

    public Task DeleteAccountAsync(int id, int userId) =>
        _accountRepository.DeleteAsync(id, userId);
}

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public Task<IEnumerable<Category>> GetAllCategoriesAsync(int userId) =>
        _categoryRepository.GetAllAsync(userId);

    public async Task<IEnumerable<Category>> GetCategoriesByTypeAsync(int userId, string type)
    {
        var categories = await _categoryRepository.GetAllAsync(userId);
        return categories.Where(c => c.Type.Equals(type, StringComparison.OrdinalIgnoreCase));
    }

    public Task<int> CreateCategoryAsync(Category category, int userId)
    {
        category.UserId = userId;
        category.Name = category.Name.Trim();
        return _categoryRepository.InsertAsync(category);
    }
}

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;

    public TransactionService(ITransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public Task<IEnumerable<Transaction>> GetAllTransactionsAsync(int userId) =>
        _transactionRepository.GetAllAsync(userId);

    public Task<Transaction?> GetTransactionByIdAsync(int id, int userId) =>
        _transactionRepository.GetByIdAsync(id, userId);

    public Task<int> CreateTransactionAsync(TransactionFormModel model, int userId)
    {
        ValidateTransaction(model);
        var transaction = MapToEntity(model, userId);
        return _transactionRepository.InsertAsync(transaction);
    }

    public async Task UpdateTransactionAsync(TransactionFormModel model, int userId)
    {
        ValidateTransaction(model);
        var existing = await _transactionRepository.GetByIdAsync(model.Id, userId)
            ?? throw new InvalidOperationException($"Transaction {model.Id} not found.");

        var transaction = MapToEntity(model, userId);
        transaction.Id = existing.Id;
        await _transactionRepository.UpdateAsync(transaction);
    }

    public Task DeleteTransactionAsync(int id, int userId) =>
        _transactionRepository.DeleteAsync(id, userId);

    public Task<IEnumerable<Transaction>> GetAllTransactionsForAdminAsync() =>
        _transactionRepository.GetAllForAdminAsync();

    private static void ValidateTransaction(TransactionFormModel model)
    {
        if (model.AccountId <= 0)
            throw new ArgumentException("Please select an account.");
        if (model.CategoryId <= 0)
            throw new ArgumentException("Please select a category.");
        if (model.Amount <= 0)
            throw new ArgumentException("Amount must be greater than zero.");
    }

    private static Transaction MapToEntity(TransactionFormModel model, int userId) => new()
    {
        Id = model.Id,
        UserId = userId,
        AccountId = model.AccountId,
        CategoryId = model.CategoryId,
        Amount = model.Amount,
        Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim(),
        TransactionDate = model.TransactionDate.Date,
        TransactionType = model.TransactionType
    };
}

public class ReportService : IReportService
{
    private readonly IReportRepository _reportRepository;

    public ReportService(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public async Task<DashboardSummary> GetDashboardSummaryAsync(int userId)
    {
        var summary = await _reportRepository.GetDashboardSummaryAsync(userId);
        return summary ?? new DashboardSummary();
    }

    public Task<IEnumerable<MonthlyReport>> GetMonthlyReportsAsync(int userId, int monthsBack = 6) =>
        _reportRepository.GetMonthlyTotalsAsync(userId, monthsBack);

    public Task<IEnumerable<CategoryBreakdown>> GetCategoryBreakdownAsync(int userId, int? month = null, int? year = null) =>
        _reportRepository.GetCategoryBreakdownAsync(userId, month, year);

    public Task<decimal> GetSavingsRateAsync(int userId, int? month = null, int? year = null) =>
        _reportRepository.GetSavingsRateAsync(userId, month, year);

    public async Task<SystemSummary> GetSystemSummaryAsync()
    {
        var summary = await _reportRepository.GetSystemSummaryAsync();
        return summary ?? new SystemSummary();
    }

    public Task<IEnumerable<MonthlyReport>> GetMonthlyReportsAllUsersAsync(int monthsBack = 6) =>
        _reportRepository.GetMonthlyTotalsAllUsersAsync(monthsBack);

    public Task<IEnumerable<CategoryBreakdown>> GetCategoryBreakdownAllUsersAsync(int? month = null, int? year = null) =>
        _reportRepository.GetCategoryBreakdownAllUsersAsync(month, year);
}

/// <summary>
/// PasswordHasher&lt;T&gt; is a standalone class with no EF dependency - using it
/// directly gets industry-standard password hashing while keeping data access
/// 100% Dapper. AppUser is only the generic type parameter it needs; the hash
/// itself is purely a function of the password plus an internal random salt.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly PasswordHasher<AppUser> _passwordHasher = new();

    public AuthService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task RegisterAsync(RegisterModel model)
    {
        var email = model.Email.Trim().ToLowerInvariant();

        var existing = await _userRepository.GetByEmailWithHashAsync(email);
        if (existing is not null)
            throw new InvalidOperationException("An account with this email already exists.");

        var hash = _passwordHasher.HashPassword(new AppUser(), model.Password);

        // Self-service registration only ever creates a 'User' account - an Admin
        // is seeded directly in the database, never created through this form.
        await _userRepository.CreateAsync(model.Name.Trim(), email, hash, "User");
    }

    public async Task<AppUser?> ValidateCredentialsAsync(
    string email,
    string password)
    {
        var credential =
            await _userRepository.GetByEmailWithHashAsync(
                email.Trim().ToLowerInvariant());

        if (credential is null ||
            !credential.IsActive ||
            string.IsNullOrEmpty(credential.PasswordHash))
        {
            return null;
        }

        var result =
            _passwordHasher.VerifyHashedPassword(
                new AppUser(),
                credential.PasswordHash,
                password);

        if (result == PasswordVerificationResult.Failed)
            return null;

        return new AppUser
        {
            Id = credential.Id,
            Name = credential.Name,
            Email = credential.Email,
            Role = credential.Role,
            IsActive = credential.IsActive,
            GoogleSubjectId = credential.GoogleSubjectId
        };
    }

    public async Task<AppUser?> AuthenticateGoogleAsync(
    string googleSubjectId,
    string name,
    string email)
    {
        googleSubjectId = googleSubjectId.Trim();
        name = name.Trim();
        email = email.Trim().ToLowerInvariant();

        // 1. Check whether this Google account is already linked
        var existingGoogleUser =
            await _userRepository.GetByGoogleSubjectIdAsync(
                googleSubjectId);

        if (existingGoogleUser is not null)
        {
            if (!existingGoogleUser.IsActive)
                return null;

            return existingGoogleUser;
        }

        // 2. Google account is not linked yet.
        // Check whether a local account already exists with this email.
        var existingCredential =
            await _userRepository.GetByEmailWithHashAsync(email);

        if (existingCredential is not null)
        {
            if (!existingCredential.IsActive)
                return null;

            // Link this Google account to the existing local account.
            await _userRepository.SetGoogleSubjectIdAsync(
                existingCredential.Id,
                googleSubjectId);

            return new AppUser
            {
                Id = existingCredential.Id,
                Name = existingCredential.Name,
                Email = existingCredential.Email,
                Role = existingCredential.Role,
                IsActive = existingCredential.IsActive,
                GoogleSubjectId = googleSubjectId
            };
        }

        // 3. No existing account exists.
        // Create a brand-new normal User account.
        var newUserId =
            await _userRepository.CreateGoogleUserAsync(
                name,
                email,
                googleSubjectId);

        return new AppUser
        {
            Id = newUserId,
            Name = name,
            Email = email,
            Role = "User",
            IsActive = true,
            GoogleSubjectId = googleSubjectId
        };
    }
}

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public Task<IEnumerable<AppUser>> GetAllUsersAsync() => _userRepository.GetAllAsync();

    public async Task SetRoleAsync(int id, string role)
    {
        // Demoting the only remaining admin would lock everyone out of the Users
        // page with no way to fix it from within the app - refuse instead.
        if (role == "User")
        {
            var target = await _userRepository.GetByIdAsync(id);
            if (target?.Role == "Admin" && await _userRepository.GetAdminCountAsync() <= 1)
                throw new InvalidOperationException("Cannot demote the only remaining admin.");
        }

        await _userRepository.SetRoleAsync(id, role);
    }

    public async Task SetActiveAsync(int id, bool isActive)
    {
        // A deactivated Admin can't sign in, so "at least one admin row exists"
        // isn't enough here - it has to be an admin who can still actually log in.
        if (!isActive)
        {
            var target = await _userRepository.GetByIdAsync(id);
            if (target?.Role == "Admin" && await _userRepository.GetActiveAdminCountAsync() <= 1)
                throw new InvalidOperationException("Cannot deactivate the only remaining active admin.");
        }

        await _userRepository.SetActiveAsync(id, isActive);
    }
}
