namespace PersonalFinanceTracker.Services;

public interface IRazorpayPaymentService
{
    Task<RazorpayOrderResult> CreateOrderAsync(
        int userId,
        string name,
        string email);

    Task<bool> VerifyPaymentAsync(
        string orderId,
        string paymentId,
        string signature);
}
