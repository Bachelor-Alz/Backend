using System.Globalization;
using HealthDevice.Services;
using Microsoft.Extensions.Logging.Abstractions;
using RichardSzalay.MockHttp;

public class GeoServiceTests
{
    public GeoServiceTests()
    {
        // Ensure consistent culture settings for tests
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
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
        var geoService = new GeoService(httpClient, NullLogger<GeoService>.Instance);

        // Act
        var address = await geoService.GetAddressFromCoordinates(57.0488, 9.9217);

        // Assert
        Assert.NotNull(address);
        Assert.Contains("Selma Lagerløfs Vej", address);
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
        var geoService = new GeoService(httpClient, NullLogger<GeoService>.Instance);

        // Act
        var location = await geoService.GetCoordinatesFromAddress("Selma Lagerløfs Vej 300", "Aalborg");

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
        var geoService = new GeoService(httpClient, NullLogger<GeoService>.Instance);

        // Act
        var location = await geoService.GetCoordinatesFromAddress("Nonexistent Street", "Nowhere");

        // Assert
        Assert.Null(location);
    }
}