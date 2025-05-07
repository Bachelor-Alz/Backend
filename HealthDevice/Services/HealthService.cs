using HealthDevice.DTO;
using HealthDevice.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StepsDTO = HealthDevice.DTO.StepsDTO;

namespace HealthDevice.Services;

public class HealthService : IHealthService
{
    private readonly ILogger<HealthService> _logger;
    private readonly IEmailService _emailService;
    private readonly IRepositoryFactory _repositoryFactory;
    private readonly IGetHealthData _getHealthDataService;
    private readonly ITimeZoneService _timeZoneService;
    public HealthService(ILogger<HealthService> logger, IRepositoryFactory repositoryFactory, IEmailService emailService, IGetHealthData getHealthDataService, ITimeZoneService timeZoneService)
    {
        _logger = logger;
        _repositoryFactory = repositoryFactory;
        _emailService = emailService;
        _getHealthDataService = getHealthDataService;
        _timeZoneService = timeZoneService;
    }
    
    public async Task<List<Heartrate>> CalculateHeartRate(DateTime currentDate, string address)
    {
        IRepository<Max30102> repository = _repositoryFactory.GetRepository<Max30102>();
        List<Max30102> heartRates = await repository.Query()
            .Where(c => c.Timestamp <= currentDate && c.MacAddress == address)
            .ToListAsync();

        if (heartRates.Count == 0)
        {
            _logger.LogWarning("No heart rate data found for elder {Address}", address);
            return new List<Heartrate>();
        }

        DateTime earliestDate = heartRates.Min(h => h.Timestamp);
        List<Heartrate> heartRateList = new();

        for (DateTime date = earliestDate; date <= currentDate; date = date.AddHours(1))
        {
            var date1 = date;
            var heartRateInHour = heartRates.Where(h => h.Timestamp >= date1 && h.Timestamp < date1.AddHours(1));
            List<Max30102> rateInHour = heartRateInHour.ToList();
            if (rateInHour.Count == 0) continue;

            _logger.LogInformation("Heart rate data found for mac-address {Address} in hour {Hour}", address, date);
            heartRateList.Add(new Heartrate
            {
                Avgrate = (int)rateInHour.Average(hr => hr.AvgHeartrate),
                Maxrate = rateInHour.Max(hr => hr.MaxHeartrate),
                Minrate = rateInHour.Min(hr => hr.MinHeartrate),
                Timestamp = new DateTime(date.Year, date.Month, date.Day, date.Hour, 0 ,0 ).ToUniversalTime(),
                MacAddress = address
            });
        }

        return heartRateList;
    }

    public async Task<List<Spo2>> CalculateSpo2(DateTime currentDate, string address)
    {
        IRepository<Max30102> repository = _repositoryFactory.GetRepository<Max30102>();
        List<Max30102> spo2Data = await repository.Query()
            .Where(c => c.Timestamp <= currentDate && c.MacAddress == address)
            .ToListAsync();

        if (spo2Data.Count == 0)
        {
            _logger.LogWarning("No SpO2 data found for elder {Address}", address);
            return new List<Spo2>();
        }

        DateTime earliestDate = spo2Data.Min(s => s.Timestamp);
        List<Spo2> spo2List = new();

        for (DateTime date = earliestDate; date <= currentDate; date = date.AddHours(1))
        {
            var hourlyData = spo2Data.Where(s => s.Timestamp >= date && s.Timestamp < date.AddHours(1)).ToList();
            if (hourlyData.Count == 0) continue;

            _logger.LogInformation("SpO2 data found for mac-address {Address} in hour {Hour}", address, date);
            spo2List.Add(new Spo2
            {
                AvgSpO2 = hourlyData.Average(sp => sp.AvgSpO2),
                MaxSpO2 = hourlyData.Max(sp => sp.MaxSpO2),
                MinSpO2 = hourlyData.Min(sp => sp.MinSpO2),
                Timestamp = new DateTime(date.Year, date.Month, date.Day, date.Hour, 0 ,0 ).ToUniversalTime(),
                MacAddress = address
            });
        }

        return spo2List;
    }

    public async Task<DistanceInfo> CalculateDistanceWalked(DateTime currentDate, string arduino)
    {
        IRepository<GPSData> repository = _repositoryFactory.GetRepository<GPSData>();
        List<GPSData> gpsData = await repository.Query()
            .Where(c => c.Timestamp.Date <= currentDate.Date && c.MacAddress == arduino)
            .ToListAsync();

        if (gpsData.Count < 2)
        {
            _logger.LogWarning("Not enough GPS data to calculate distance for elder {Arduino}", arduino);
            return new DistanceInfo();
        }

        float distance = 0;
        for (int i = 0; i < gpsData.Count - 1; i++)
        {
            double a = Math.Pow(Math.Sin((gpsData[i + 1].Latitude - gpsData[i].Latitude) / 2), 2) +
                       Math.Cos(gpsData[i].Latitude) * Math.Cos(gpsData[i + 1].Latitude) *
                       Math.Pow(Math.Sin((gpsData[i + 1].Longitude - gpsData[i].Longitude) / 2), 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            distance += (float)(6371 * c); // Earth's radius in kilometers
        }

        if (distance == 0)
        {
            _logger.LogWarning("No distance data found for elder {Arduino}", arduino);
            return new DistanceInfo();
        }

        _logger.LogInformation("Distance data found for elder {Arduino}", arduino);
        return new DistanceInfo
        {
            Distance = distance,
            Timestamp = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, currentDate.Hour, currentDate.Minute, 0)
        };
    }

   public async Task DeleteMax30102Data(DateTime currentDate, string arduino)
{
    IRepository<Max30102> repository = _repositoryFactory.GetRepository<Max30102>();
    List<Max30102> data = await repository.Query()
        .Where(c => c.Timestamp <= currentDate && c.MacAddress == arduino)
        .ToListAsync();

    if (data.Count == 0)
    {
        _logger.LogWarning("No Max30102 data found to delete for elder {Arduino}", arduino);
        return;
    }

    await repository.RemoveRange(data);
    _logger.LogInformation("Deleted {Count} Max30102 records for elder {Arduino}", data.Count, arduino);
}
    public async Task DeleteGpsData(DateTime currentDate, string arduino)
    {
        IRepository<GPSData> repository = _repositoryFactory.GetRepository<GPSData>();
        List<GPSData> data = await repository.Query()
            .Where(c => c.Timestamp <= currentDate && c.MacAddress == arduino)
            .ToListAsync();

        if (data.Count == 0)
        {
            _logger.LogWarning("No GPS data found to delete for elder {Arduino}", arduino);
            return;
        }

        await repository.RemoveRange(data);
        _logger.LogInformation("Deleted {Count} GPS records for elder {Arduino}", data.Count, arduino);
    }

    public async Task ComputeOutOfPerimeter(string Arduino, Location location)
    {
        IRepository<Perimeter> perimeterRepository = _repositoryFactory.GetRepository<Perimeter>();
        IRepository<Elder> elderRepository = _repositoryFactory.GetRepository<Elder>();

        Perimeter? perimeter = await perimeterRepository.Query()
            .FirstOrDefaultAsync(p => p.MacAddress == Arduino);
        if (perimeter == null)
        {
            _logger.LogWarning("No perimeter found for elder with Arduino {Arduino}", Arduino);
            return;
        }

        Elder? elder = await elderRepository.Query()
            .FirstOrDefaultAsync(e => e.MacAddress == Arduino);
        if (elder == null)
        {
            _logger.LogWarning("Elder with Arduino {Arduino} not found", Arduino);
            return;
        }
        if (perimeter.Latitude == null || perimeter.Longitude == null) return;
        double dLat = (perimeter.Latitude.Value - location.Latitude) * Math.PI / 180;
        double dLon = (perimeter.Longitude.Value - location.Longitude) * Math.PI / 180;
        double lat1 = location.Latitude * Math.PI / 180;
        double lat2 = perimeter.Latitude.Value * Math.PI / 180;

        double a = Math.Pow(Math.Sin(dLat / 2), 2) +
                   Math.Cos(lat1) * Math.Cos(lat2) *
                   Math.Pow(Math.Sin(dLon / 2), 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        double d = 6371 * c;

        Console.WriteLine($"Location: ({location.Latitude}, {location.Longitude})");
        Console.WriteLine($"Perimeter: ({perimeter.Latitude.Value}, {perimeter.Longitude.Value})");

        _logger.LogInformation("Distance from perimeter: {Distance}", d);
        if (elder.outOfPerimeter)
        {
            if (d < perimeter.Radius)
            {
                elder.outOfPerimeter = false;
                await elderRepository.Update(elder);
                _logger.LogInformation("Elder {Email} is back in perimeter", elder.Email);
                return;
            }
            _logger.LogInformation("Elder {Email} is already out of perimeter", elder.Email);
            return;
        }
        if (d > perimeter.Radius)
        {
            _logger.LogInformation("Elder {Email} is out of perimeter", elder.Email);
            elder.outOfPerimeter = true;
            await elderRepository.Update(elder);
        }
    }

    public async Task<Location> GetLocation(DateTime currentTime, string arduino)
    {
        IRepository<GPSData> repository = _repositoryFactory.GetRepository<GPSData>();
        GPSData? gpsData = await repository.Query()
            .Where(c => c.Timestamp <= currentTime && c.MacAddress == arduino)
            .OrderByDescending(c => c.Timestamp)
            .FirstOrDefaultAsync();

        if (gpsData != null)
        {
            _logger.LogInformation("GPS data found for elder {Arduino} at {Time}", arduino, currentTime);
            return new Location
            {
                Latitude = gpsData.Latitude,
                Longitude = gpsData.Longitude,
                Timestamp = gpsData.Timestamp
            };
        }

        _logger.LogWarning("No GPS data found for elder {Arduino}", arduino);
        return new Location();
    }

    public async Task<ActionResult> SetPerimeter(int radius, string elderEmail)
    {
        IRepository<Elder> elderRepository = _repositoryFactory.GetRepository<Elder>();
        IRepository<Perimeter> perimeterRepository = _repositoryFactory.GetRepository<Perimeter>();
        IRepository<Caregiver> caregiverRepository = _repositoryFactory.GetRepository<Caregiver>();
        Elder? elder = await elderRepository.Query().FirstOrDefaultAsync(m => m.Email == elderEmail);
            _logger.LogInformation("Setting perimeter for elder: {ElderEmail}", elderEmail);
            if (elder is null)
            {
                _logger.LogError("Elder not found for email: {ElderEmail}", elderEmail);
                return new BadRequestObjectResult("Elder not found.");
            }
            if (string.IsNullOrEmpty(elder.MacAddress))
            {
                _logger.LogError("Elder Arduino not set for elder: {ElderEmail}", elderEmail);
                return new BadRequestObjectResult("Elder Arduino not set.");
            }
            if (radius < 0)
            {
                _logger.LogError("Invalid radius value: {Radius}", radius);
                return new BadRequestObjectResult("Invalid radius value.");
            }
            _logger.LogInformation("Setting perimeter for elder: {ElderEmail}", elderEmail);
            Perimeter? oldPerimeter = await perimeterRepository.Query().FirstOrDefaultAsync(m => m.MacAddress == elder.MacAddress);
            if (oldPerimeter == null)
            {
                _logger.LogInformation("Creating new perimeter for elder: {ElderEmail}", elderEmail);
                Perimeter perimeter = new Perimeter
                {
                    Latitude = elder.latitude,
                    Longitude = elder.longitude,
                    Radius = radius,
                    MacAddress = elder.MacAddress
                };
                await perimeterRepository.Add(perimeter);
            }
            else
            {
                _logger.LogInformation("Updating existing perimeter for elder: {ElderEmail}", elderEmail);
                oldPerimeter = new Perimeter
                {
                    Latitude = elder.latitude,
                    Longitude = elder.longitude,
                    Radius = radius,
                    MacAddress = elder.MacAddress
                };
                await perimeterRepository.Update(oldPerimeter);
                
                // Send email to caregiver
                List<Caregiver> caregivers = await caregiverRepository.Query()
                    .Where(c => c.Elders != null && c.Elders.Any(e => e.Id == elder.Id))
                    .ToListAsync();
                foreach (Caregiver caregiver in caregivers)
                {
                    Email emailInfo = new Email { name = caregiver.Name, email = caregiver.Email };
                    _logger.LogInformation("Sending email to {CaregiverEmail}", caregiver.Email);
                    await _emailService.SendEmail(emailInfo, "Elder changed their perimeter", $"Elder {elder.Name} changed their perimeter to {radius} kilometers.");
                }
            }
            return new OkObjectResult("Perimeter set successfully");
    }

    public async Task<ActionResult<List<ElderLocationDTO>>> GetEldersLocation(string email)
    {
        IRepository<Caregiver> caregiverRepository = _repositoryFactory.GetRepository<Caregiver>();
        IRepository<Location> locationRepository = _repositoryFactory.GetRepository<Location>();
        IRepository<Perimeter> perimeterRepository = _repositoryFactory.GetRepository<Perimeter>();
         Caregiver? caregiver = await caregiverRepository.Query()
                .Include(c => c.Elders)
                .FirstOrDefaultAsync(c => c.Email == email);
            if (caregiver == null)
            {
                _logger.LogError("Caregiver not found.");
                return new BadRequestObjectResult("Caregiver not found.");
            }
            List<Elder>? elders = caregiver.Elders;
            if (elders == null || elders.Count == 0)
            {
                _logger.LogError("No elders found for the caregiver.");
                return new BadRequestObjectResult("No elders found for the caregiver.");
            }
            _logger.LogInformation("Found {ElderCount} elders for caregiver: {CaregiverEmail}", elders.Count, email);
            List<ElderLocationDTO> elderLocations = [];
            foreach (Elder elder in elders)
            {
                if (string.IsNullOrEmpty(elder.MacAddress))
                {
                    _logger.LogError("Elder Arduino not set for elder: {ElderEmail}", elder.Email);
                    continue;
                }
                _logger.LogInformation("Fetching location data for elder: {ElderEmail}", elder.Email);
                Location? location = await locationRepository.Query().FirstOrDefaultAsync(m => m.MacAddress == elder.MacAddress);
                if (location == null) continue;
                {
                    _logger.LogInformation("Fetched location data for elder: {ElderEmail}", elder.Email);
                    if (elder.Email == null) continue;
                    _logger.LogInformation("Fetching perimeter data for elder: {ElderEmail}", elder.Email);
                    Perimeter? perimeter = await perimeterRepository.Query().FirstOrDefaultAsync(m => m.MacAddress == elder.MacAddress);
                    if (perimeter != null)
                    {
                        _logger.LogInformation("Fetched perimeter data for elder: {ElderEmail}", elder.Email);
                        elderLocations.Add(new ElderLocationDTO
                        {
                            email = elder.Email,
                            name = elder.Name,
                            latitude = location.Latitude,
                            longitude = location.Longitude,
                            lastUpdated = location.Timestamp,
                            perimeter = new Perimeter
                            {
                                Latitude = perimeter.Latitude,
                                Longitude = perimeter.Longitude,
                                Radius = perimeter.Radius
                            }
                        });
                    }
                    else
                    {
                        _logger.LogInformation("No perimeter data found for elder: {ElderEmail}", elder.Email);
                        elderLocations.Add(new ElderLocationDTO
                        {
                            email = elder.Email,
                            name = elder.Name,
                            latitude = location.Latitude,
                            longitude = location.Longitude,
                            lastUpdated = location.Timestamp
                        });
                    }
                }
            }
            if (elderLocations.Count == 0)
            {
                _logger.LogError("No location data found for the elders.");
                return new BadRequestObjectResult("No location data found for the elders.");
            }
            _logger.LogInformation("Found {LocationCount} locations for the elders.", elderLocations.Count);
            return elderLocations;
    }

    public async Task<ActionResult<List<FallDTO>>> GetFalls(string elderEmail, DateTime date, Period period, TimeZoneInfo timezone)
    {
        switch (period)
        {
            case Period.Hour:
            {
                _logger.LogInformation("Processing current fall data for elder: {ElderEmail}", elderEmail);
                DateTime newTime = new DateTime(date.Year, date.Month, date.Day, date.Hour + 1, 0, 0).ToUniversalTime();
                List<FallInfo> data = await _getHealthDataService.GetHealthData<FallInfo>(
                    elderEmail, period, newTime, timezone);
                _logger.LogInformation("Fetched fall data: {Count}", data.Count);
                List<FallDTO> result = data.Select(fall => new FallDTO { Timestamp = _timeZoneService.UTCToLocalTime(timezone,fall.Timestamp), fallCount = 1 })
                    .ToList();
                _logger.LogInformation("Processed fall data: {Count}", result.Count);
                return result.Count != 0 ? result : [];
            }
            case Period.Day:
            {
                _logger.LogInformation("Processing daily fall data for elder: {ElderEmail}", elderEmail);
                DateTime newTime = new DateTime(date.Year, date.Month, date.Day, 23, 59, 59).ToUniversalTime();
                List<FallInfo> data = await _getHealthDataService.GetHealthData<FallInfo>(
                    elderEmail, period, newTime, timezone);
                // Group by the hour and select the latest fall for each hour and count the falls in that hour for each data point found in the hour
                List<FallDTO> result = data.Where(t => t.Timestamp.Date >= date.Date.AddHours(23).AddMinutes(59).AddSeconds(59))
                    .GroupBy(f => f.Timestamp.Hour)
                    .Select(g => new FallDTO
                    {
                        Timestamp = _timeZoneService.UTCToLocalTime(timezone,g.OrderByDescending(f => f.Timestamp.Hour).First().Timestamp),
                        fallCount = g.Count()
                    }).ToList();

// Add missing days with no falls
                DateTime startDate = new DateTime(newTime.Year, date.Month, date.Day, 0, 0 , 0); // Adjust based on the period
                DateTime endDate = date.Date.AddHours(23).AddMinutes(59).AddSeconds(59); // Adjust based on the period
                for (DateTime currentDate = startDate; currentDate < endDate; currentDate = currentDate.AddHours(1))
                {
                    if (result.All(r => r.Timestamp.Hour != currentDate.Hour))
                    {
                        result.Add(new FallDTO
                        {
                            Timestamp = _timeZoneService.UTCToLocalTime(timezone, currentDate.AddHours(-2)),
                            fallCount = 0
                        });
                    }
                }

                return result.Count != 0 ? result.OrderBy(r => r.Timestamp.Hour).ToList() : [];
            }
            default:
            {
                _logger.LogInformation("Processing daily fall data for elder: {ElderEmail}", elderEmail);
                //Find the end of the week the date is in 
                DateTime endOfWeek = date.AddDays(7 - (int)date.DayOfWeek).Date;
                DateTime newTime = new DateTime(endOfWeek.Year, endOfWeek.Month, endOfWeek.Day, 23, 59, 59).ToUniversalTime();
                List<FallInfo> data = await _getHealthDataService.GetHealthData<FallInfo>(
                    elderEmail, period, newTime, timezone);
                List<FallDTO> result = data.Where(t => t.Timestamp.Date <= endOfWeek.Date)
                    .GroupBy(f => f.Timestamp.Date)
                    .Select(g => new FallDTO
                    {
                        Timestamp = _timeZoneService.UTCToLocalTime(timezone, g.Key),
                        fallCount = g.Count()
                    }).ToList();

// Add missing days with no falls
                DateTime startDate = endOfWeek.Date - TimeSpan.FromDays(6); // Adjust based on the period
                for (DateTime currentDate = startDate; currentDate < date.Date; currentDate = currentDate.AddDays(1))
                {
                    if (result.All(r => r.Timestamp.Date != currentDate.Date))
                    {
                        result.Add(new FallDTO
                        {
                            Timestamp = _timeZoneService.UTCToLocalTime(timezone, currentDate),
                            fallCount = 0
                        });
                    }
                }

                return result.Count != 0 ? result.OrderBy(r => r.Timestamp.Date).ToList() : [];
            }
        }
    }

    public async Task<ActionResult<List<StepsDTO>>> GetSteps(string elderEmail, DateTime date, Period period, TimeZoneInfo timezone)
    {
        switch (period)
        {
            case Period.Hour:
            {
                _logger.LogInformation("Processing current steps data for elder: {ElderEmail}", elderEmail);
                DateTime newTime = new DateTime(date.Year, date.Month, date.Day, date.Hour, 59, 59).ToUniversalTime();
                List<Steps> data = await _getHealthDataService.GetHealthData<Steps>(
                    elderEmail, period, newTime, timezone);
                _logger.LogInformation("Fetched steps data: {Count}", data.Count);
                List<StepsDTO> steps = data.GroupBy(t => t.Timestamp).Select(s => new StepsDTO
                {
                    Timestamp = _timeZoneService.UTCToLocalTime(timezone, s.Key),
                    StepsCount = s.Sum(c => c.StepsCount)
                }).OrderBy(t => t.Timestamp).ToList();
                return steps.Count != 0 ? steps : [];
            }
            case Period.Day:
            {
                _logger.LogInformation("Processing daily steps data for elder: {ElderEmail}", elderEmail);
                DateTime startTime = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0).ToUniversalTime();
                DateTime endTime = new DateTime(date.Year, date.Month, date.Day, 23, 59, 59).ToUniversalTime();
                List<Steps> data = await _getHealthDataService.GetHealthData<Steps>(
                    elderEmail, period, endTime, timezone);
                List<StepsDTO> result = Enumerable.Range(0, 24) // Ensure all 24 hours are included
                    .Select(hour => new StepsDTO
                    {
                        Timestamp = _timeZoneService.UTCToLocalTime(timezone, startTime.AddHours(hour)),
                        StepsCount = data.Where(s => s.Timestamp.Hour == hour).Sum(s => s.StepsCount)
                    }).ToList();
                _logger.LogInformation("Fetched steps data: {Count}", data.Count);
                return result.Count != 0 ? result : [];
            }
            default:
            {
                _logger.LogInformation("Processing weekly steps data for elder: {ElderEmail}", elderEmail);
                DateTime endOfWeek = date.AddDays(7 - (int)date.DayOfWeek).Date;
                DateTime newTime = new DateTime(endOfWeek.Year, endOfWeek.Month, endOfWeek.Day, 23, 59, 59).ToUniversalTime();// End of the week
                List<Steps> data = await _getHealthDataService.GetHealthData<Steps>(
                    elderEmail, period, newTime, timezone);
                List<StepsDTO> result = data.Where(t => t.Timestamp.Date <= endOfWeek.Date)
                    .GroupBy(s => s.Timestamp.Date) // Group by the date
                    .Select(g => new StepsDTO
                    {
                        Timestamp = _timeZoneService.UTCToLocalTime(timezone, g.Key), // Use the date as the timestamp
                        StepsCount = g.Sum(s => s.StepsCount) // Sum the steps for each day
                    }).ToList();
                _logger.LogInformation("Fetched steps data: {Count}", data.Count);
                return result.Count != 0 ? result : [];
            }
        }
    }

    public async Task<ActionResult<List<DistanceInfoDTO>>> GetDistance(string elderEmail, DateTime date, Period period, TimeZoneInfo timezone)
    {
        switch (period)
        {
            case Period.Hour:
            {
                _logger.LogInformation("Processing current distance data for elder: {ElderEmail}", elderEmail);
                DateTime newTime = new DateTime(date.Year, date.Month, date.Day, date.Hour, 59, 59).ToUniversalTime();
                List<DistanceInfo> data = await _getHealthDataService.GetHealthData<DistanceInfo>(
                    elderEmail, period, newTime, timezone);
                List<DistanceInfoDTO> distance = data.GroupBy(t => t.Timestamp).Select(s => new DistanceInfoDTO
                {
                    Timestamp = _timeZoneService.UTCToLocalTime(timezone, s.Key),
                    Distance = s.Sum(c => c.Distance)
                }).OrderBy(t => t.Timestamp).ToList();
                _logger.LogInformation("Fetched distance data: {Count}", data.Count);
                return distance.Count != 0 ? distance : [];
            }
            case Period.Day:
            {
                _logger.LogInformation("Processing daily distance data for elder: {ElderEmail}", elderEmail);
                DateTime startTime = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0).ToUniversalTime();
                DateTime endTime = new DateTime(date.Year, date.Month, date.Day, 23, 59, 59).ToUniversalTime();
                List<DistanceInfo> data = await _getHealthDataService.GetHealthData<DistanceInfo>(
                    elderEmail, period, endTime, timezone);
                List<DistanceInfoDTO> result = Enumerable.Range(0, 24) // Ensure all 24 hours are included
                    .Select(hour => new DistanceInfoDTO
                    {
                        Timestamp = _timeZoneService.UTCToLocalTime(timezone, startTime.AddHours(hour)),
                        Distance = data.Where(d => d.Timestamp.Hour == hour).Sum(d => d.Distance)
                    }).ToList();
                _logger.LogInformation("Fetched distance data: {Count}", data.Count);
                return result.Count != 0 ? result : [];
            }
            default:
            {
                _logger.LogInformation("Processing weekly distance data for elder: {ElderEmail}", elderEmail);
                DateTime endOfWeek = date.AddDays(7 - (int)date.DayOfWeek).Date;
                DateTime newTime = new DateTime(endOfWeek.Year, endOfWeek.Month, endOfWeek.Day, 23, 59, 59).ToUniversalTime();// End of the week
                List<DistanceInfo> data = await _getHealthDataService.GetHealthData<DistanceInfo>(
                    elderEmail, period, newTime, timezone);
                List<DistanceInfoDTO> result = data.Where(t => t.Timestamp.Date <= endOfWeek.Date)
                    .GroupBy(s => s.Timestamp.Date) // Group by the date
                    .Select(g => new DistanceInfoDTO
                    {
                        Timestamp = _timeZoneService.UTCToLocalTime(timezone, g.Key), // Use the date as the timestamp
                        Distance = g.Sum(s => s.Distance),
                    }).ToList();
                _logger.LogInformation("Fetched distance data: {Count}", data.Count);
                return result.Count != 0 ? result : [];
            }
        }
    }

    public async Task<ActionResult<List<PostHeartRate>>> GetHeartrate(string elderEmail, DateTime date, Period period, TimeZoneInfo timezone)
{
    DateTime newTime;
    switch (period)
    {
        case Period.Hour:
            newTime = new DateTime(date.Year, date.Month, date.Day, date.Hour, 59, 59);
            break;
        case Period.Day:
            newTime = new DateTime(date.Year, date.Month, date.Day, 23, 59, 59);
            break;
        case Period.Week:
            DateTime endOfWeek = date.AddDays(7 - (int)date.DayOfWeek).Date;
            newTime = new DateTime(endOfWeek.Year, endOfWeek.Month, endOfWeek.Day, 23, 59, 59);// End of the week
            _logger.LogInformation("Time, {Time}", newTime);
            break;
        default:
            _logger.LogError("Invalid period specified: {Period}", period);
            return new BadRequestObjectResult("Invalid period specified. Valid values are 'Hour', 'Day', or 'Week'.");
    }
    _logger.LogInformation("Time {Time}", newTime);

    // Fetch historical heart rate data
    List<Heartrate> data = await _getHealthDataService.GetHealthData<Heartrate>(
        elderEmail, period, newTime, timezone);
    _logger.LogInformation("Fetched historical heart rate data: {Count}", data.Count);

    // Fetch current heart rate data if historical data is unavailable
    List<Max30102> currentHeartRateData =
        await _getHealthDataService.GetHealthData<Max30102>(elderEmail, period, newTime, timezone);
    _logger.LogInformation("Fetched current heart rate data: {Count}", currentHeartRateData.Count);

    if (data.Count != 0 && currentHeartRateData.Count < 7)
    {
        _logger.LogInformation("Processing historical heart rate data for elder: {ElderEmail}", elderEmail);
        switch (period)
        {
            case Period.Hour:
                return data.GroupBy(t => t.Timestamp).Select(hr => new PostHeartRate
                {
                    Avgrate = (int)hr.Average(h => h.Avgrate),
                    Maxrate = hr.Max(h => h.Maxrate),
                    Minrate = hr.Min(h => h.Minrate),
                    Timestamp = _timeZoneService.UTCToLocalTime(timezone, hr.Key),
                    MacAddress = hr.First().MacAddress
                }).OrderBy(t => t.Timestamp).ToList();
            case Period.Day:
                return data.GroupBy(t => t.Timestamp.Hour).Select(hr => new PostHeartRate
                {
                    Avgrate = (int)hr.Average(h => h.Avgrate),
                    Maxrate = hr.Max(h => h.Maxrate),
                    Minrate = hr.Min(h => h.Minrate),
                    Timestamp = _timeZoneService.UTCToLocalTime(timezone,newTime.Date.AddHours(hr.Key)),
                    MacAddress = hr.First().MacAddress
                }).OrderBy(t => t.Timestamp).ToList();
            default:
                return data.GroupBy(t => t.Timestamp.Date).Select(hr => new PostHeartRate
                {
                    Avgrate = (int)hr.Average(h => h.Avgrate),
                    Maxrate = hr.Max(h => h.Maxrate),
                    Minrate = hr.Min(h => h.Minrate),
                    Timestamp = _timeZoneService.UTCToLocalTime(timezone, hr.Key),
                    MacAddress = hr.First().MacAddress
                }).OrderBy(t => t.Timestamp).ToList();
        }
    }

    if (currentHeartRateData.Count == 0)
    {
        return new List<PostHeartRate>();
    }
    
    
    // Process current heart rate data based on the period
    List<PostHeartRate> processedHeartrates = new();
    switch (period)
    {
        case Period.Hour:
            processedHeartrates.AddRange(currentHeartRateData.Select(g => new PostHeartRate
            {
                Avgrate = g.AvgHeartrate,
                Maxrate = g.MaxHeartrate,
                Minrate = g.MinHeartrate,
                Timestamp = _timeZoneService.UTCToLocalTime(timezone, g.Timestamp),
                MacAddress = g.MacAddress
            }));
            break;
        case Period.Day:
            processedHeartrates.AddRange(currentHeartRateData
                .GroupBy(h => h.Timestamp.Hour)
                .Select(g => new PostHeartRate()
                {
                    Avgrate = (int)g.Average(h => h.AvgHeartrate),
                    Maxrate = g.Max(h => h.MaxHeartrate),
                    Minrate = g.Min(h => h.MinHeartrate),
                    Timestamp = _timeZoneService.UTCToLocalTime(timezone,newTime.Date.AddHours(g.Key)), 
                    MacAddress = g.First().MacAddress
                }));
            break;
        case Period.Week:
            processedHeartrates.AddRange(currentHeartRateData
                .GroupBy(h => h.Timestamp.Date)
                .Select(g => new PostHeartRate()
                {
                    Avgrate = (int)g.Average(h => h.AvgHeartrate),
                    Maxrate = g.Max(h => h.MaxHeartrate),
                    Minrate = g.Min(h => h.MinHeartrate),
                    Timestamp = _timeZoneService.UTCToLocalTime(timezone, g.Key),
                    MacAddress = g.First().MacAddress
                }));
            // Add missing days with heart rates from `data` for the missing days' timestamp
            DateTime startDate = newTime.Date.AddDays(-6); // Assuming the week starts 6 days before the given date
            DateTime endDate = newTime.Date;

            for (DateTime currentDate = startDate; currentDate <= endDate; currentDate = currentDate.AddDays(1))
            {
                if (processedHeartrates.All(hr => hr.Timestamp.Date != currentDate.Date))
                {
                    var fallbackData = data.Where(d => d.Timestamp.Date == currentDate.Date).ToList();
                    if (fallbackData.Count != 0)
                    {
                        processedHeartrates.Add(new PostHeartRate
                        {
                            Avgrate = (int)fallbackData.Average(h => h.Avgrate),
                            Maxrate = fallbackData.Max(h => h.Maxrate),
                            Minrate = fallbackData.Min(h => h.Minrate),
                            Timestamp = _timeZoneService.UTCToLocalTime(timezone, currentDate),
                            MacAddress = fallbackData.First().MacAddress
                        });
                    }
                }
            }
            processedHeartrates = processedHeartrates.Where(t => t.Timestamp.Date <= endDate.Date).ToList();
            break;
    }

    _logger.LogInformation("ProcessedData {Count}", processedHeartrates.Count);
    return processedHeartrates.OrderBy(t => t.Timestamp).ToList();
}

    public async Task<ActionResult<List<PostSpO2>>> GetSpO2(string elderEmail, DateTime date, Period period, TimeZoneInfo timezone)
{
    DateTime newTime;
    switch (period)
    {
        case Period.Hour:
            newTime = new DateTime(date.Year, date.Month, date.Day, date.Hour, 59, 59).ToUniversalTime();
            break;
        case Period.Day:
            newTime = new DateTime(date.Year, date.Month, date.Day, 23, 59, 59).ToUniversalTime();
            break;
        case Period.Week:
            DateTime endOfWeek = date.AddDays(7 - (int)date.DayOfWeek).Date;
            newTime = new DateTime(endOfWeek.Year, endOfWeek.Month, endOfWeek.Day, 23, 59, 59).ToUniversalTime();
            break;
        default:
            _logger.LogError("Invalid period specified: {Period}", period);
            return new BadRequestObjectResult("Invalid period specified. Valid values are 'Hour', 'Day', or 'Week'.");
    }

    _logger.LogInformation("Time {Time}", newTime);

    // Fetch historical SpO2 data
    List<Spo2> data = await _getHealthDataService.GetHealthData<Spo2>(
        elderEmail, period, newTime, timezone);
    _logger.LogInformation("Fetched historical SpO2 data: {Count}", data.Count);

    // Fetch current SpO2 data if historical data is unavailable
    List<Max30102> currentSpo2Data =
        await _getHealthDataService.GetHealthData<Max30102>(elderEmail, period, newTime, timezone);
    _logger.LogInformation("Fetched current SpO2 data: {Count}", currentSpo2Data.Count);

    if (data.Count != 0 && currentSpo2Data.Count < 7)
    {
        _logger.LogInformation("Processing historical SpO2 data for elder: {ElderEmail}", elderEmail);
        switch (period)
        {
            case Period.Hour:
                return data.GroupBy(t => t.Timestamp).Select(sp => new PostSpO2
                {
                    AvgSpO2 =sp.Average(h => h.AvgSpO2),
                    MaxSpO2 = sp.Max(s => s.MaxSpO2),
                    MinSpO2 = sp.Min(s => s.MinSpO2),
                    Timestamp = _timeZoneService.UTCToLocalTime(timezone, sp.Key),
                    MacAddress = sp.First().MacAddress
                }).OrderBy(t => t.Timestamp).ToList();
            case Period.Day:
                return data.GroupBy(t => t.Timestamp.Hour).Select(sp => new PostSpO2
                {
                    AvgSpO2 =sp.Average(h => h.AvgSpO2),
                    MaxSpO2 = sp.Max(s => s.MaxSpO2),
                    MinSpO2 = sp.Min(s => s.MinSpO2),
                    Timestamp = _timeZoneService.UTCToLocalTime(timezone,newTime.Date.AddHours(sp.Key)), 
                    MacAddress = sp.First().MacAddress
                }).OrderBy(t => t.Timestamp.Hour).ToList();
            default:
                return data.GroupBy(t => t.Timestamp.Date).Select(sp => new PostSpO2
                {
                    AvgSpO2 =sp.Average(h => h.AvgSpO2),
                    MaxSpO2 = sp.Max(s => s.MaxSpO2),
                    MinSpO2 = sp.Min(s => s.MinSpO2),
                    Timestamp = _timeZoneService.UTCToLocalTime(timezone, sp.Key),
                    MacAddress = sp.First().MacAddress
                }).OrderBy(t => t.Timestamp.Date).ToList();
        }
    }

    if (currentSpo2Data.Count == 0)
    {
        return new List<PostSpO2>();
    }

    // Process current SpO2 data based on the period
    List<PostSpO2> processedSpo2 = new();
    switch (period)
    {
        case Period.Hour:
            processedSpo2.AddRange(currentSpo2Data.Select(g => new PostSpO2
            {
                AvgSpO2 = g.AvgSpO2,
                MaxSpO2 = g.MaxSpO2,
                MinSpO2 = g.MinSpO2,
                Timestamp = _timeZoneService.UTCToLocalTime(timezone, g.Timestamp),
                MacAddress = g.MacAddress
            }));
            break;
        case Period.Day:
            processedSpo2.AddRange(currentSpo2Data
                .GroupBy(s => s.Timestamp.Hour)
                .Select(g => new PostSpO2
                {
                    AvgSpO2 = g.Average(s => s.AvgSpO2),
                    MaxSpO2 = g.Max(s => s.MaxSpO2),
                    MinSpO2 = g.Min(s => s.MinSpO2),
                    Timestamp = _timeZoneService.UTCToLocalTime(timezone,newTime.Date.AddHours(g.Key)), 
                    MacAddress = g.First().MacAddress
                }));
            break;
        case Period.Week:
            processedSpo2.AddRange(currentSpo2Data
                .GroupBy(s => s.Timestamp.Date)
                .Select(g => new PostSpO2
                {
                    AvgSpO2 = g.Average(s => s.AvgSpO2),
                    MaxSpO2 = g.Max(s => s.MaxSpO2),
                    MinSpO2 = g.Min(s => s.MinSpO2),
                    Timestamp = _timeZoneService.UTCToLocalTime(timezone, g.Key),
                    MacAddress = g.First().MacAddress
                }));
            
            // Add missing days with heart rates from `data` for the missing days' timestamp
            DateTime startDate = newTime.Date.AddDays(-6); // Assuming the week starts 6 days before the given date
            DateTime endDate = newTime.Date;

            for (DateTime currentDate = startDate; currentDate <= endDate; currentDate = currentDate.AddDays(1))
            {
                if (processedSpo2.All(hr => hr.Timestamp.Date != currentDate.Date))
                {
                    var fallbackData = data.Where(d => d.Timestamp.Date == currentDate.Date).ToList();
                    if (fallbackData.Count != 0)
                    {
                        processedSpo2.Add(new PostSpO2
                        {
                            AvgSpO2 = fallbackData.Average(h => h.AvgSpO2),
                            MaxSpO2 = fallbackData.Max(h => h.MaxSpO2),
                            MinSpO2 = fallbackData.Min(h => h.MinSpO2),
                            Timestamp = _timeZoneService.UTCToLocalTime(timezone, currentDate),
                            MacAddress = fallbackData.First().MacAddress
                        });
                    }
                }
            }
            processedSpo2 = processedSpo2.Where(t => t.Timestamp.Date <= endDate.Date).ToList();
            break; 
    }

    _logger.LogInformation("ProcessedData {Count}", processedSpo2.Count);
    return processedSpo2.OrderBy(t => t.Timestamp).ToList();
}
}