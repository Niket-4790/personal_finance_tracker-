using PersonalFinanceTracker.Models;
namespace PersonalFinanceTracker.Services;

public interface INotificationService
{
    Task<IEnumerable<Notification>> GetNotificationsAsync(int userId);
    Task<int> GetUnreadCountAsync(int userId);

    Task CreateNotificationAsync(int userId, string title, string message, bool sendEmail = false);

    Task MarkAsReadAsync(int id, int userId);
    Task MarkAllAsReadAsync(int userId);
    Task DeleteNotificationAsync(int id, int userId);
}