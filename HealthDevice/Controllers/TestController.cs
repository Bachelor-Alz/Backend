using HealthDevice.DTO;
using HealthDevice.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HealthDevice.Controllers;

[Route("api/[controller]")]
[ApiController]
//This controller is used to test what ever endpoint we what to test if it works without implementing it in a main controller
public class TestController : ControllerBase
{
    private readonly GeoService _geoService;
    private readonly UserManager<Elder> _elderManager;
    
    public TestController(GeoService geoService, UserManager<Elder> elderManager)
    {
        _geoService = geoService;
        _elderManager = elderManager;
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
    [HttpPost("FakeData")]
    public async Task<ActionResult> GenerateFakeData(Elder elder)
    {
        DateTime currentDate = DateTime.Now;
        int HeartrangeX = 40;
        int HeartrangeY = 200;
        double SpO2rangeX = 0.7;
        double SpO2rangeY = 1;
        
        if (elder.MAX30102Data == null)
        {
            elder.MAX30102Data = new List<Max30102>();
        }
        
        for (int i = 0; i < 30000; i++)
        {
            DateTime timestamp = currentDate.AddMinutes(i);
            int heartrate = Random.Shared.Next(HeartrangeX, HeartrangeY);
            double spo2 = Random.Shared.NextDouble() * (SpO2rangeY - SpO2rangeX) + SpO2rangeX;
            elder.MAX30102Data.Add(new Max30102
            {
                Id = i,
                Heartrate = heartrate,
                SpO2 = (float)spo2,
                Timestamp = timestamp
            });
        }
        await _elderManager.UpdateAsync(elder);
        return Ok("Fake data generated successfully");
    }
}