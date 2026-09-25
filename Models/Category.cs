namespace PersonalFinanceTracker.Models;

public class Category
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public string Color { get; set; } = "#6366f1";
}