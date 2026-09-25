
using PersonalFinanceTracker.Models;

namespace PersonalFinanceTracker.Services;

public interface IAccountService
{
    Task<IEnumerable<Account>> GetAllAccountsAsync(int userId);

    Task<Account?> GetAccountByIdAsync(int id,int userId);

    Task<int> CreateAccountAsync(AccountFormModel model,int userId);

    Task UpdateAccountAsync(AccountFormModel model,int userId);

    Task DeleteAccountAsync(int id,int userId);
}

