using System.Text.Json;

namespace HealthDevice.Services;

public class GeoService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GeoService> _logger;
    
    public GeoService(HttpClient httpClient, ILogger<GeoService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }
    
    public async Task<string> GetAddressFromCoordinates(double latitude, double longitude)
    {
        string url = $"https://nominatim.openstreetmap.org/reverse?format=json&lat={latitude}&lon={longitude}&zoom=18&addressdetails=1";
        HttpResponseMessage response = await _httpClient.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            string json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<NominatimResponse>(json);
            return data?.DisplayName ?? "Unknown address";
        }

        _logger.LogWarning("Failed to get address for coordinates {latitude}, {longitude}", latitude, longitude);
        return "Unknown address";
    }
}

public class NominatimResponse
{
    public string DisplayName { get; set; }
}