using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.Repositories;

namespace PersonalFinanceTracker.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repository;
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;

    public NotificationService(
        INotificationRepository repository,
        IUserRepository userRepository,
        IEmailService emailService)
    {
        _repository = repository;
        _userRepository = userRepository;
        _emailService = emailService;
    }

    public Task<IEnumerable<Notification>> GetNotificationsAsync(
        int userId)
    {
        return _repository.GetAllAsync(userId);
    }

    public Task<int> GetUnreadCountAsync(int userId)
    {
        return _repository.GetUnreadCountAsync(userId);
    }

    public async Task CreateNotificationAsync(int userId,string title, string message,bool sendEmail = false)
    {
        await _repository.CreateAsync(
            new Notification
            {
                UserId = userId,
                Title = title,
                Message = message
            });

        if (!sendEmail)
            return;

        var user =await _userRepository.GetByIdAsync(userId);

        if (user is null ||
            string.IsNullOrWhiteSpace(user.Email))
        {
            return;
        }

        await _emailService.SendAsync(
            user.Email,
            title,
            message);
    }

    public Task MarkAsReadAsync(
        int id,
        int userId)
    {
        return _repository.MarkAsReadAsync(id, userId);
    }

    public Task MarkAllAsReadAsync(int userId)
    {
        return _repository.MarkAllAsReadAsync(userId);
    }

    public Task DeleteNotificationAsync(
        int id,
        int userId)
    {
        return _repository.DeleteAsync(id, userId);
    }
}