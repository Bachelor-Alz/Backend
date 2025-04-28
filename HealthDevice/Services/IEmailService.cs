using HealthDevice.DTO;

namespace HealthDevice.Services;

public interface IEmailService
{
    Task SendEmail(Email to, string subject, string body);
}