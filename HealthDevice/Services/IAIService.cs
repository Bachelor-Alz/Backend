namespace HealthDevice.Services;

public interface IAIService
{
    Task HandleAiRequest(List<int> request, string address);
}