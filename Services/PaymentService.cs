using PersonalFinanceTracker.Repositories;

namespace PersonalFinanceTracker.Services;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;

    public PaymentService(
        IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<bool> IsPremiumAsync(int userId)
    {
        var payment =
            await _paymentRepository.GetLatestPaidPaymentAsync(
                userId);

        return payment is not null
            && payment.Status == "Paid";
    }
}
