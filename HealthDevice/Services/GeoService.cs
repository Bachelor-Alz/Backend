using System.Text.Json;
using System.Text.Json.Serialization;
using HealthDevice.Models;

namespace HealthDevice.Services;

public class GeoService : IGeoService
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
        return data != null ? FormatAddress(data) : "Unknown location";
    }

    public async Task<Location?> GetCoordinatesFromAddress(string street, string city)
    {
        Dictionary<string, string?> queryParams = new()
        {
            ["street"] = street,
            ["city"] = city,
        };

        string query = string.Join("&", queryParams
            .Where(kvp => !string.IsNullOrWhiteSpace(kvp.Value))
            .Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value ?? string.Empty)}"));

        string url = $"https://nominatim.openstreetmap.org/search?format=json&{query}";
        string json = await (await _httpClient.GetAsync(url)).Content.ReadAsStringAsync();
        

        List<NominatimSearchResponse>? data = JsonSerializer.Deserialize<List<NominatimSearchResponse>>(json);
        string[]? box = data?.FirstOrDefault()?.BoundingBox;

        if (box?.Length >= 4 && double.TryParse(box[0], out double lat) && double.TryParse(box[2], out double lon))
        {
            _logger.LogInformation("Coordinates from Geo API: {lat}, {lon}", lat, lon);
            return new Location { Latitude = lat, Longitude = lon };
        }

        _logger.LogWarning("No coordinates found for address: {address}", query);
        return null;
    }


    private string FormatAddress(NominatimResponse response)
    {
        _logger.LogInformation("Processing DisplayName: {DisplayName}", response.DisplayName);
        if (string.IsNullOrWhiteSpace(response.DisplayName))
        {
            return "Unknown location";
        }

        string[] addressParts = response.DisplayName.Split(", ");

        // Ensure the array has enough parts to avoid IndexOutOfRangeException
        if (addressParts.Length < 5)
        {
            _logger.LogWarning("Unexpected address format: {DisplayName}", response.DisplayName);
            return "Unknown location";
        }

        string houseNumber = addressParts.Length > 0 ? addressParts[0] : "Unknown";
        string street = addressParts.Length > 1 ? addressParts[1] : "Unknown";
        string city = addressParts.Length > 3 ? addressParts[3] : "Unknown";
        string postalCode = addressParts.Length > 6 ? addressParts[6] : "Unknown";
        string country = addressParts.Length > 7 ? addressParts[7] : "Unknown";

        string formattedAddress = $"{street} {houseNumber}, {city}, {postalCode}, {country}";
        return formattedAddress;
    }

    public static float CalculateDistance(Location locationA, Location locationB)
    {
        double dLat = (locationA.Latitude - locationB.Latitude) * Math.PI / 180;
        double dLon = (locationA.Longitude - locationB.Longitude) * Math.PI / 180;
        double lat1 = locationB.Latitude * Math.PI / 180;
        double lat2 = locationA.Latitude * Math.PI / 180;

        double a = Math.Pow(Math.Sin(dLat / 2), 2) +
                   Math.Cos(lat1) * Math.Cos(lat2) *
                   Math.Pow(Math.Sin(dLon / 2), 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        float d = (float)(6371 * c);

        return d; // Distance in kilometers
    }
}

public class NominatimResponse
{
    [JsonPropertyName("display_name")]
    public string? DisplayName { get; init; }
}

public class NominatimSearchResponse
{

    [JsonPropertyName("boundingbox")]
    public string[]? BoundingBox { get; set; }
}