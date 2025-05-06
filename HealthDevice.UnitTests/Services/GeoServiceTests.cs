using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using HealthDevice.DTO;
using HealthDevice.Services;
using Microsoft.Extensions.Logging.Abstractions;
using RichardSzalay.MockHttp;
using Xunit;

public class GeoServiceTests
{
    public GeoServiceTests()
    {
        // Ensure consistent culture settings for tests
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
    }

    [Fact]
    public void CalculateDistance_ShouldReturnCorrectDistance()
    {
        // Arrange
        var location1 = new Location { Latitude = 57.0124, Longitude = 9.9915 }; // Selma Lagerløfs Vej 300, 9220 Aalborg
        var location2 = new Location { Latitude = 57.0235, Longitude = 9.9771 }; // Ribevej 3, 9220 Aalborg
        var expectedDistance = 1.5; // Approximate distance in kilometers

        // Act
        var distance = GeoService.CalculateDistance(location1, location2);

        // Assert
        Assert.InRange(distance, expectedDistance - 0.05, expectedDistance + 0.05); // Allowing a small margin of error
    }

    [Fact]
    public void CalculateDistance_IdenticalLocations_ShouldReturnZero()
    {
        // Arrange
        var location = new Location { Latitude = 57.0488, Longitude = 9.9217 };

        // Act
        var distance = GeoService.CalculateDistance(location, location);

        // Assert
        Assert.Equal(0, distance);
    }

    [Fact]
    public async Task GetAddressFromCoordinates_ShouldReturnValidAddress()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("https://nominatim.openstreetmap.org/reverse")
            .WithQueryString("format=json&lat=57.0488&lon=9.9217&zoom=18&addressdetails=1")
            .Respond("application/json", "{\"display_name\":\"300, Selma Lagerløfs Vej, x, Aalborg, x, x, 9220, Denmark\"}");

        var httpClient = mockHttp.ToHttpClient();
        var logger = NullLogger<GeoService>.Instance;
        var geoService = new GeoService(httpClient, logger);

        var latitude = 57.0488;
        var longitude = 9.9217;

        // Act
        var address = await geoService.GetAddressFromCoordinates(latitude, longitude);

        // Assert
        Assert.NotNull(address);
        Assert.Contains("Selma Lagerløfs Vej 300", address);
        Assert.Contains("Aalborg", address);
        Assert.Contains("9220", address);
    }

    [Fact]
    public async Task GetCoordinatesFromAddress_ShouldReturnValidCoordinates()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("https://nominatim.openstreetmap.org/search")
            .WithQueryString("format=json&street=Selma%20Lagerløfs%20Vej%20300&city=Aalborg")
            .Respond("application/json", "[{\"boundingbox\":[\"57.0488\",\"57.0489\",\"9.9216\",\"9.9217\"]}]");

        var httpClient = mockHttp.ToHttpClient();
        var logger = NullLogger<GeoService>.Instance;
        var geoService = new GeoService(httpClient, logger);

        var street = "Selma Lagerløfs Vej 300";
        var city = "Aalborg";

        // Act
        var location = await geoService.GetCoordinatesFromAddress(street, city);

        // Assert
        Assert.NotNull(location);
        Assert.InRange(location!.Latitude, 57.0487, 57.0490);
        Assert.InRange(location.Longitude, 9.9215, 9.9218);
    }

    [Fact]
    public async Task GetCoordinatesFromAddress_NoResults_ShouldReturnNull()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("https://nominatim.openstreetmap.org/search")
            .WithQueryString("format=json&street=Nonexistent%20Street&city=Nowhere")
            .Respond("application/json", "[]");

        var httpClient = mockHttp.ToHttpClient();
        var logger = NullLogger<GeoService>.Instance;
        var geoService = new GeoService(httpClient, logger);

        var street = "Nonexistent Street";
        var city = "Nowhere";

        // Act
        var location = await geoService.GetCoordinatesFromAddress(street, city);

        // Assert
        Assert.Null(location);
    }
}