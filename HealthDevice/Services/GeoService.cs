using System.Text.Json;
using System.Text.Json.Serialization;
using HealthDevice.DTO;


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

    public async Task<Location?> GetCoordinatesFromAddress(string street, string city, string state, string country, string postalCode = null, string amenity = null)
    {
        var queryParams = new Dictionary<string, string?>
        {
            ["street"] = street,
            ["city"] = city,
            ["state"] = state,
            ["country"] = country,
            ["postalcode"] = postalCode,
            ["amenity"] = amenity
        };

        string query = string.Join("&", queryParams
            .Where(kvp => !string.IsNullOrWhiteSpace(kvp.Value))
            .Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value!)}"));

        string url = $"https://nominatim.openstreetmap.org/search?format=json&{query}";
        string json = await (await _httpClient.GetAsync(url)).Content.ReadAsStringAsync();

        _logger.LogInformation("Response from Nominatim: {json}", json);

        var data = JsonSerializer.Deserialize<List<NominatimSearchResponse>>(json);
        string[]? box = data?.FirstOrDefault()?.BoundingBox;

        if (box?.Length >= 4 && double.TryParse(box[0], out double lat) && double.TryParse(box[2], out double lon))
        {
            _logger.LogInformation("Coordinates from bounding box: {lat}, {lon}", lat, lon);
            return new Location { Latitude = lat, Longitude = lon };
        }

        _logger.LogWarning("No coordinates found for address: {address}", query);
        return null;
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

public class NominatimSearchResponse
{
    [JsonPropertyName("lat")]
    public string Lat { get; set; }

    [JsonPropertyName("lon")]
    public string Lon { get; set; }

    [JsonPropertyName("boundingbox")]
    public string[] BoundingBox { get; set; }
}