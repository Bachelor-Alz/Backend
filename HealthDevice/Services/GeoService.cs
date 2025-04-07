using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HealthDevice.Services;

public class GeoService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GeoService> _logger;

    public GeoService(HttpClient httpClient, ILogger<GeoService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("HealthDeviceApp/1.0 (daniel.r.bechl@gmail.com)");
    }

    public async Task<string> GetAddressFromCoordinates(double latitude, double longitude)
    {
        string url = $"https://nominatim.openstreetmap.org/reverse?format=json&lat={latitude}&lon={longitude}&zoom=18&addressdetails=1";
        HttpResponseMessage response = await _httpClient.GetAsync(url);
        string json = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<NominatimResponse>(json);
        if(data != null)
            return FormatAddress(data);
        return "Unknown location";
    }

    private string FormatAddress(NominatimResponse response)
    {
        string[] addressParts = response.DisplayName.Split(", ");
        string houseNumber = addressParts[0];
        string street = addressParts[1];
        string city = addressParts[3];
        string postalCode = addressParts[6];
        string country = addressParts[7];
        string formattedAddress = $"{street} {houseNumber}, {city}, {postalCode}, {country}";
        return formattedAddress;
    }
}

public class NominatimResponse
{
    [JsonPropertyName("display_name")]
    public string DisplayName { get; set; }
}