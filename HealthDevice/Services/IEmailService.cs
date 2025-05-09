using HealthDevice.Models;

namespace HealthDevice.Services;

public interface IEmailService
{
    Task SendEmail(string subject, string body, Elder elder);
}