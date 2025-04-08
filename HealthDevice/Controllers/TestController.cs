using HealthDevice.Services;
using Microsoft.AspNetCore.Mvc;

namespace HealthDevice.Controllers;

[Route("api/[controller]")]
[ApiController]
//This controller is used to test what ever endpoint we what to test if it works without implementing it in a main controller
public class TestController : ControllerBase
{
    private readonly GeoService _geoService;
    
    public TestController(GeoService geoService)
    {
        _geoService = geoService;
    }
    
    [HttpPost("Address")]
    public async Task<ActionResult> GetAddress(double latitude, double longitude)
    {
        var result = await _geoService.GetAddressFromCoordinates(latitude, longitude);
        return Ok(result);
    }
    
    [HttpPost("Coordinates")]
    public async Task<ActionResult> GetCoordinates(string street, string city, string state, string country, string? postalCode = null, string? amenity = null)
    {
        var result = await _geoService.GetCoordinatesFromAddress(street, city, state, country, postalCode, amenity);
        return Ok(result);
    }
}