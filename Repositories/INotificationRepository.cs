using PersonalFinanceTracker.Models;

namespace PersonalFinanceTracker.Repositories;

public interface INotificationRepository
{
    Task<IEnumerable<Notification>> GetAllAsync(int userId);
    Task<int> GetUnreadCountAsync(int userId);
    Task<int> CreateAsync(Notification notification);
    Task MarkAsReadAsync(int id, int userId);
    Task MarkAllAsReadAsync(int userId);

    Task DeleteAsync(int id, int userId);
}