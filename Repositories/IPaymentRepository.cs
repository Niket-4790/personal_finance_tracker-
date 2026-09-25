using PersonalFinanceTracker.Models;

namespace PersonalFinanceTracker.Repositories;

public interface IPaymentRepository
{
    Task<int> CreateAsync(Payment payment);

    Task<Payment?> GetByOrderIdAsync(string razorpayOrderId);

    Task MarkAsPaidAsync( string razorpayOrderId, string razorpayPaymentId);

    Task<Payment?> GetLatestPaidPaymentAsync(int userId);
}