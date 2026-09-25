using Dapper;
using PersonalFinanceTracker.Data;
using PersonalFinanceTracker.Models;
using System.Data;

namespace PersonalFinanceTracker.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public NotificationRepository(
        IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Notification>> GetAllAsync(int userId)
    {
        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryAsync<Notification>(
            "dbo.usp_Notification_GetAll",
            new { UserId = userId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<int> GetUnreadCountAsync(int userId)
    {
        using var connection = _connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<int>(
            "dbo.usp_Notification_GetUnreadCount",
            new { UserId = userId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<int> CreateAsync(Notification notification)
    {
        using var connection = _connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<int>(
            "dbo.usp_Notification_Insert",
            new
            {
                notification.UserId,
                notification.Title,
                notification.Message
            },
            commandType: CommandType.StoredProcedure);
    }

    public async Task MarkAsReadAsync(int id, int userId)
    {
        using var connection = _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            "dbo.usp_Notification_MarkAsRead",
            new
            {
                Id = id,
                UserId = userId
            },
            commandType: CommandType.StoredProcedure);
    }

    public async Task MarkAllAsReadAsync(int userId)
    {
        using var connection = _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            "dbo.usp_Notification_MarkAllAsRead",
            new { UserId = userId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task DeleteAsync(int id, int userId)
    {
        using var connection =
            _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            "dbo.usp_Notification_Delete",
            new
            {
                Id = id,
                UserId = userId
            },
            commandType: CommandType.StoredProcedure);
    }
}