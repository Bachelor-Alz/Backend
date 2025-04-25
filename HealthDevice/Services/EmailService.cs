using HealthDevice.DTO;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace HealthDevice.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly string smtpHost;
    private readonly int smtpPort;
    private readonly string smtpUser;
    private readonly string smtpPassword;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
        smtpHost = Environment.GetEnvironmentVariable("SMTP_HOST") ?? throw new ArgumentNullException("SMTP_HOST environment variable is not set.");
        smtpPort = int.TryParse(Environment.GetEnvironmentVariable("SMTP_PORT"), out var port) ? port : throw new ArgumentNullException("SMTP_PORT environment variable is not set or invalid.");
        smtpUser = Environment.GetEnvironmentVariable("SMTP_USER") ?? throw new ArgumentNullException("SMTP_USER environment variable is not set.");
        smtpPassword = Environment.GetEnvironmentVariable("SMTP_PASSWORD") ?? throw new ArgumentNullException("SMTP_PASSWORD environment variable is not set.");
    }

    public async Task SendEmail(Email to, string subject, string body)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Health Device", smtpUser));
        message.To.Add(new MailboxAddress(to.name, to.email));
        message.Subject = subject;
        message.Body = new TextPart("plain")
        {
            Text = body
        };

        using var client = new SmtpClient();

        try
        {
            await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(smtpUser, smtpPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email sent to {Email} with subject {Subject}.", to.email, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {Email} with subject {Subject}.", to.email, subject);
        }
    }
}