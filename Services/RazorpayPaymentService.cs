using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.Repositories;
using Razorpay.Api;
using System.Security.Cryptography;
using System.Text;

namespace PersonalFinanceTracker.Services;
public class RazorpayPaymentService : IRazorpayPaymentService
{
    private readonly IConfiguration _configuration;
    private readonly IPaymentRepository _paymentRepository;

    public RazorpayPaymentService(
        IConfiguration configuration,
        IPaymentRepository paymentRepository)
    {
        _configuration = configuration;
        _paymentRepository = paymentRepository;
    }

    public async Task<RazorpayOrderResult> CreateOrderAsync(
        int userId,
        string name,
        string email)
    {
        var keyId = _configuration["Razorpay:KeyId"]
            ?? throw new InvalidOperationException(
                "Razorpay KeyId is not configured.");

        var keySecret = _configuration["Razorpay:KeySecret"]
            ?? throw new InvalidOperationException(
                "Razorpay KeySecret is not configured.");

        var client = new RazorpayClient(keyId, keySecret);

        // ₹99 = 9900 paise
        var orderData =
            new Dictionary<string, object>
            {
                ["amount"] = 9900,
                ["currency"] = "INR",
                ["receipt"] =
                    $"premium_{userId}_{DateTime.UtcNow.Ticks}",

                ["notes"] = new Dictionary<string, object>
                {
                    ["app_user_id"] = userId.ToString(),
                    ["name"] = name,
                    ["email"] = email
                }
            };

        var order = client.Order.Create(orderData);

        var orderId = order["id"]?.ToString();

        if (string.IsNullOrWhiteSpace(orderId))
        {
            throw new InvalidOperationException(
                "Razorpay did not return an order ID.");
        }

        // Save the order before opening Razorpay Checkout.
        await _paymentRepository.CreateAsync(
            new PersonalFinanceTracker.Models.Payment
            {
                UserId = userId,
                RazorpayOrderId = orderId,
                Amount = 99,
                Status = "Created"
            });

        return new RazorpayOrderResult(
            orderId,
            9900,
            "INR",
            keyId);
    }

    public async Task<bool> VerifyPaymentAsync(
    string orderId,
    string paymentId,
    string signature)
    {
        var keySecret =  _configuration["Razorpay:KeySecret"]
            ?? throw new InvalidOperationException(
                "Razorpay KeySecret is not configured.");

        var payload = $"{orderId}|{paymentId}";

        using var hmac =
            new HMACSHA256(
                Encoding.UTF8.GetBytes(keySecret));

        var hash =
            hmac.ComputeHash(
                Encoding.UTF8.GetBytes(payload));

        var expectedSignature =
            Convert.ToHexString(hash)
                .ToLowerInvariant();

        var isValid =
            CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(
                    expectedSignature),
                Encoding.UTF8.GetBytes(
                    signature));

        if (!isValid)
            return false;

        // Payment signature is valid.
        await _paymentRepository.MarkAsPaidAsync(
            orderId,
            paymentId);

        return true;
    }
}

public record RazorpayOrderResult(
    string OrderId,
    int Amount,
    string Currency,
    string KeyId);