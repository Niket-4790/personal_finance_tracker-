using System.Net;
using System.Net.Mail;

namespace PersonalFinanceTracker.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendAsync( string to, string subject,string body)
    {
        var host =
            _configuration["Email:SmtpHost"]
            ?? throw new InvalidOperationException(
                "Email SMTP host is not configured.");

        var port =
            int.Parse(
                _configuration["Email:SmtpPort"] ?? "587");

        var username =
            _configuration["Email:Username"]
            ?? throw new InvalidOperationException(
                "Email username is not configured.");

        var password =
            _configuration["Email:Password"]
            ?? throw new InvalidOperationException(
                "Email password is not configured.");

        var from =
            _configuration["Email:From"]
            ?? username;

        using var message = new MailMessage();

        message.From = new MailAddress(from);
        message.To.Add(to);
        message.Subject = subject;
        message.Body = body;
        message.IsBodyHtml = false;

        using var client = new SmtpClient(host, port);

        client.EnableSsl = true;
        client.Credentials =
            new NetworkCredential(
                username,
                password);

        await client.SendMailAsync(message);
    }
}