using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.Repositories;

namespace PersonalFinanceTracker.Services;

public class AccountService : IAccountService
{
    private readonly IAccountRepository _accountRepository;
    private readonly IPaymentService _paymentService;

    public AccountService(
     IAccountRepository accountRepository,
     IPaymentService paymentService)
    {
        _accountRepository = accountRepository;
        _paymentService = paymentService;
    }

    public Task<IEnumerable<Account>> GetAllAccountsAsync(int userId) =>
        _accountRepository.GetAllAsync(userId);

    public Task<Account?> GetAccountByIdAsync(int id, int userId) =>
        _accountRepository.GetByIdAsync(id, userId);

    public async Task<int> CreateAccountAsync(
    AccountFormModel model,
    int userId)
    {
        var isPremium = await _paymentService.IsPremiumAsync(userId);

        if (!isPremium)
        {
            var existingAccounts = await _accountRepository.GetAllAsync(userId);

            if (existingAccounts.Count() >= 1)
            {
                throw new InvalidOperationException(
                    "Free plan allows only 1 account. Upgrade to Premium to create unlimited accounts.");
            }
        }

        var account = new Account
        {
            UserId = userId,
            Name = model.Name.Trim(),
            AccountType = model.AccountType,
            Balance = model.InitialBalance
        };

        return await _accountRepository.InsertAsync(account);
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
