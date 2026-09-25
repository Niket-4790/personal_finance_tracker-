namespace PersonalFinanceTracker.Models;

public class Payment
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string RazorpayOrderId { get; set; } = string.Empty;

    public string? RazorpayPaymentId { get; set; }

    public decimal Amount { get; set; }

    public string Status { get; set; } = "Created";

    public DateTime? PaidAt { get; set; }

    public DateTime CreatedAt { get; set; }
}