
using PersonalFinanceTracker.Models;

namespace PersonalFinanceTracker.Services;

public interface IEmailService
{
    Task SendAsync(
        string to,
        string subject,
        string body);
}