using HealthDevice.DTO;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace HealthDevice.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUser;
    private readonly string _smtpPassword;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
        _smtpHost = Environment.GetEnvironmentVariable("SMTP_HOST") ?? "";
        _smtpPort = int.TryParse(Environment.GetEnvironmentVariable("SMTP_PORT"), out int port) ? port : 0;
        _smtpUser = Environment.GetEnvironmentVariable("SMTP_USER") ?? "";
        _smtpPassword = Environment.GetEnvironmentVariable("SMTP_PASSWORD") ?? "";
    }

    public async Task SendEmail(Email to, string subject, string body)
    {
        if(_smtpHost == "" || _smtpPort == 0 || _smtpUser == "" || _smtpPassword == "")
        {
            _logger.LogWarning("SMTP configuration is not set. Email will not be sent.");
            return;
        }
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Health Device", _smtpUser));
        message.To.Add(new MailboxAddress(to.Name, to.EmailAddress));
        message.Subject = subject;
        message.Body = new TextPart("plain")
        {
            Text = body
        };

        using var client = new SmtpClient();
        
        try
        {
            await client.ConnectAsync(_smtpHost, _smtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_smtpUser, _smtpPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email sent to {Email} with subject {Subject}.", to.EmailAddress, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending Email to {Email} with subject {Subject}.", to.EmailAddress, subject);
        }
    }
}