namespace PersonalFinanceTracker.Services;

public interface IPaymentService
{
    Task<bool> IsPremiumAsync(int userId);
}
