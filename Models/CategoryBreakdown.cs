namespace PersonalFinanceTracker.Models;

public class CategoryBreakdown
{
    public string CategoryName { get; set; } = string.Empty;

    public string CategoryType { get; set; } = string.Empty;

    public string CategoryColor { get; set; } = "#6366f1";

    public decimal TotalAmount { get; set; }
}