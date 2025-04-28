using HealthDevice.DTO;

namespace HealthDevice.Services;

public interface IGeoService
{
    Task<string> GetAddressFromCoordinates(double latitude, double longitude);
    Task<Location?> GetCoordinatesFromAddress(string street, string city);
}