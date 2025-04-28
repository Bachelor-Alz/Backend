using HealthDevice.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthDevice.Services;

public class HealthService : IHealthService
{
    private readonly ILogger<HealthService> _logger;
    private readonly IEmailService _emailService;
    private readonly IRepositoryFactory _repositoryFactory;
    private readonly IGetHealthData _getHealthDataService;
    public HealthService(ILogger<HealthService> logger, IRepositoryFactory repositoryFactory, IEmailService emailService, IGetHealthData getHealthDataService)
    {
        _logger = logger;
        _repositoryFactory = repositoryFactory;
        _emailService = emailService;
        _getHealthDataService = getHealthDataService;
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
                Avgrate = (int)rateInHour.Average(h => h.Heartrate),
                Maxrate = rateInHour.Max(h => h.Heartrate),
                Minrate = rateInHour.Min(h => h.Heartrate),
                Timestamp = date
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
                AvgSpO2 = hourlyData.Average(s => s.SpO2),
                MaxSpO2 = hourlyData.Max(s => s.SpO2),
                MinSpO2 = hourlyData.Min(s => s.SpO2),
                Timestamp = date
            });
        }

        return spo2List;
    }

    public async Task<Kilometer> CalculateDistanceWalked(DateTime currentDate, string arduino)
    {
        IRepository<GPS> repository = _repositoryFactory.GetRepository<GPS>();
        List<GPS> gpsData = await repository.Query()
            .Where(c => c.Timestamp.Date <= currentDate.Date && c.MacAddress == arduino)
            .ToListAsync();

        if (gpsData.Count < 2)
        {
            _logger.LogWarning("Not enough GPS data to calculate distance for elder {Arduino}", arduino);
            return new Kilometer();
        }

        double distance = 0;
        for (int i = 0; i < gpsData.Count - 1; i++)
        {
            double a = Math.Pow(Math.Sin((gpsData[i + 1].Latitude - gpsData[i].Latitude) / 2), 2) +
                       Math.Cos(gpsData[i].Latitude) * Math.Cos(gpsData[i + 1].Latitude) *
                       Math.Pow(Math.Sin((gpsData[i + 1].Longitude - gpsData[i].Longitude) / 2), 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            distance += 6371 * c; // Earth's radius in kilometers
        }

        if (distance == 0)
        {
            _logger.LogWarning("No distance data found for elder {Arduino}", arduino);
            return new Kilometer();
        }

        _logger.LogInformation("Distance data found for elder {Arduino}", arduino);
        return new Kilometer
        {
            Distance = distance,
            Timestamp = currentDate
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
        IRepository<GPS> repository = _repositoryFactory.GetRepository<GPS>();
        List<GPS> data = await repository.Query()
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
        IRepository<GPS> repository = _repositoryFactory.GetRepository<GPS>();
        GPS? gpsData = await repository.Query()
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

    public async Task<ActionResult<List<ElderLocation>>> GetEldersLocation(string email)
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
            List<ElderLocation> elderLocations = [];
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
                        elderLocations.Add(new ElderLocation
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
                        elderLocations.Add(new ElderLocation
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

    public async Task<ActionResult<List<FallDTO>>> GetFalls(string elderEmail, DateTime date, Period period)
    {
        if (period == Period.Hour)
        {
            _logger.LogInformation("Processing current fall data for elder: {ElderEmail}", elderEmail);
            DateTime newTime = new DateTime(date.Year, date.Month, date.Day, date.Hour + 1, 0, 0).ToUniversalTime();
            List<FallInfo> data = await _getHealthDataService.GetHealthData<FallInfo>(
                elderEmail, period, newTime);
            _logger.LogInformation("Fetched fall data: {Count}", data.Count);
            List<FallDTO> result = data.Select(fall => new FallDTO { Timestamp = fall.Timestamp, fallCount = 1 })
                .ToList();
            _logger.LogInformation("Processed fall data: {Count}", result.Count);
            return result.Count != 0 ? result : [];
        }

        if (Period.Day == period)
        {
            _logger.LogInformation("Processing daily fall data for elder: {ElderEmail}", elderEmail);
            DateTime newTime = new DateTime(date.Year, date.Month, date.Day + 1, 0, 0, 0).ToUniversalTime();
            List<FallInfo> data = await _getHealthDataService.GetHealthData<FallInfo>(
                elderEmail, period, newTime);
            // Group by the hour and select the latest fall for each hour and count the falls in that hour for each data point found in the hour
            List<FallDTO> result = data
                .GroupBy(f => f.Timestamp.Hour)
                .Select(g => new FallDTO
                {
                    Timestamp = g.OrderByDescending(f => f.Timestamp.Hour).First().Timestamp,
                    fallCount = g.Count()
                }).ToList();

// Add missing days with no falls
            DateTime startDate = date.Date - TimeSpan.FromDays(1); // Adjust based on the period
            DateTime endDate = date.Date; // Adjust based on the period
            for (DateTime currentDate = startDate; currentDate < endDate; currentDate = currentDate.AddHours(1))
            {
                if (result.All(r => r.Timestamp.Hour != currentDate.Hour))
                {
                    result.Add(new FallDTO
                    {
                        Timestamp = currentDate,
                        fallCount = 0
                    });
                }
            }

            return result.OrderBy(r => r.Timestamp.Hour).ToList();
        }
        else
        {
            _logger.LogInformation("Processing daily fall data for elder: {ElderEmail}", elderEmail);
            DateTime newTime = new DateTime(date.Year, date.Month, date.Day + 1, 0, 0, 0).ToUniversalTime();
            List<FallInfo> data = await _getHealthDataService.GetHealthData<FallInfo>(
                elderEmail, period, newTime);
            List<FallDTO> result = data
                .GroupBy(f => f.Timestamp.Date)
                .Select(g => new FallDTO
                {
                    Timestamp = g.OrderByDescending(f => f.Timestamp).First().Timestamp.Date,
                    fallCount = g.Count()
                }).ToList();

// Add missing days with no falls
            DateTime startDate = date.Date - TimeSpan.FromDays(7); // Adjust based on the period
            DateTime endDate = date.Date; // Adjust based on the period
            for (DateTime currentDate = startDate; currentDate < endDate; currentDate = currentDate.AddDays(1))
            {
                if (result.All(r => r.Timestamp.Date != currentDate.Date))
                {
                    result.Add(new FallDTO
                    {
                        Timestamp = currentDate,
                        fallCount = 0
                    });
                }
            }

            return result.OrderBy(r => r.Timestamp.Date).ToList();
        }
    }

    public async Task<ActionResult<List<Steps>>> GetSteps(string elderEmail, DateTime date, Period period)
    {
        switch (period)
        {
            case Period.Hour:
            {
                _logger.LogInformation("Processing current steps data for elder: {ElderEmail}", elderEmail);
                DateTime newTime = new DateTime(date.Year, date.Month, date.Day, date.Hour + 1, 0, 0).ToUniversalTime();
                List<Steps> data = await _getHealthDataService.GetHealthData<Steps>(
                    elderEmail, period, newTime);
                _logger.LogInformation("Fetched steps data: {Count}", data.Count);
                return data.Count != 0 ? data : [];
            }
            case Period.Day:
            {
                _logger.LogInformation("Processing daily steps data for elder: {ElderEmail}", elderEmail);
                DateTime startTime = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0).ToUniversalTime();
                DateTime endTime = new DateTime(date.Year, date.Month, date.Day, 23, 59, 59).ToUniversalTime();
                List<Steps> data = await _getHealthDataService.GetHealthData<Steps>(
                    elderEmail, period, endTime);
                List<Steps> result = Enumerable.Range(0, 24) // Ensure all 24 hours are included
                    .Select(hour => new Steps
                    {
                        Timestamp = startTime.AddHours(hour),
                        StepsCount = data.Where(s => s.Timestamp.Hour == hour).Sum(s => s.StepsCount)
                    }).ToList();
                _logger.LogInformation("Fetched steps data: {Count}", data.Count);
                return result.Count != 0 ? result : [];
            }
            default:
            {
                _logger.LogInformation("Processing weekly steps data for elder: {ElderEmail}", elderEmail);
                DateTime startTime = date.Date.ToUniversalTime();
                DateTime endTime = startTime.AddDays(7).AddSeconds(-1); // End of the week
                List<Steps> data = await _getHealthDataService.GetHealthData<Steps>(
                    elderEmail, period, endTime);
                List<Steps> result = data.Where(t => t.Timestamp.Date >= startTime && t.Timestamp.Date <= endTime)
                    .GroupBy(s => s.Timestamp.Date) // Group by the date
                    .Select(g => new Steps
                    {
                        Timestamp = g.Key, // Use the date as the timestamp
                        StepsCount = g.Sum(s => s.StepsCount) // Sum the steps for each day
                    }).ToList();
                _logger.LogInformation("Fetched steps data: {Count}", data.Count);
                return result.Count != 0 ? result : [];
            }
        }
    }

    public async Task<ActionResult<List<Kilometer>>> GetDistance(string elderEmail, DateTime date, Period period)
    {
        switch (period)
        {
            case Period.Hour:
            {
                _logger.LogInformation("Processing current distance data for elder: {ElderEmail}", elderEmail);
                DateTime newTime = new DateTime(date.Year, date.Month, date.Day, date.Hour + 1, 0, 0).ToUniversalTime();
                List<Kilometer> data = await _getHealthDataService.GetHealthData<Kilometer>(
                    elderEmail, period, newTime);
                _logger.LogInformation("Fetched distance data: {Count}", data.Count);
                return data.Count != 0 ? data : [];
            }
            case Period.Day:
            {
                _logger.LogInformation("Processing daily distance data for elder: {ElderEmail}", elderEmail);
                DateTime startTime = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0).ToUniversalTime();
                DateTime endTime = new DateTime(date.Year, date.Month, date.Day, 23, 59, 59).ToUniversalTime();
                List<Kilometer> data = await _getHealthDataService.GetHealthData<Kilometer>(
                    elderEmail, period, endTime);
                List<Kilometer> result = Enumerable.Range(0, 24) // Ensure all 24 hours are included
                    .Select(hour => new Kilometer
                    {
                        Timestamp = startTime.AddHours(hour),
                        Distance = data.Where(d => d.Timestamp.Hour == hour).Sum(d => d.Distance)
                    }).ToList();
                _logger.LogInformation("Fetched distance data: {Count}", data.Count);
                return result.Count != 0 ? result : [];
            }
            default:
            {
                _logger.LogInformation("Processing weekly distance data for elder: {ElderEmail}", elderEmail);
                DateTime startTime = date.Date.ToUniversalTime();
                DateTime endTime = startTime.AddDays(7).AddSeconds(-1); // End of the week
                List<Kilometer> data = await _getHealthDataService.GetHealthData<Kilometer>(
                    elderEmail, period, endTime);
                List<Kilometer> result = data.Where(t => t.Timestamp.Date >= startTime && t.Timestamp.Date <= endTime)
                    .GroupBy(s => s.Timestamp.Date) // Group by the date
                    .Select(g => new Kilometer
                    {
                        Timestamp = g.Key, // Use the date as the timestamp
                        Distance = g.Sum(s => s.Distance),
                    }).ToList();
                _logger.LogInformation("Fetched distance data: {Count}", data.Count);
                return result.Count != 0 ? result : [];
            }
        }
    }

    public async Task<ActionResult<List<PostHeartRate>>> GetHeartrate(string elderEmail, DateTime date, Period period)
    {
        // Fetch historical heart rate data
            List<Heartrate> data = await _getHealthDataService.GetHealthData<Heartrate>(
                elderEmail, period, date.ToUniversalTime());
            _logger.LogInformation("Fetched historical heart rate data: {Count}", data.Count);
            // Fetch current heart rate data if historical data is unavailable
            List<Max30102> currentHeartRateData =
                await _getHealthDataService.GetHealthData<Max30102>(elderEmail, period, date.ToUniversalTime());
            _logger.LogInformation("Fetched current heart rate data: {Count}", currentHeartRateData.Count);

            var newestHr = currentHeartRateData.OrderByDescending(h => h.Timestamp).First();

            if (data.Count != 0)
            {
                _logger.LogInformation("Processing historical heart rate data for elder: {ElderEmail}", elderEmail);
                return data.Select(hr =>
                    new PostHeartRate
                    {
                        CurrentHeartRate = new currentHeartRate
                        {
                            Heartrate = newestHr.Heartrate,
                            Timestamp = hr.Timestamp
                        },
                        Heartrate = new Heartrate
                        {
                            Avgrate = hr.Avgrate,
                            Maxrate = hr.Maxrate,
                            Minrate = hr.Minrate,
                            Timestamp = hr.Timestamp
                        }
                    }).ToList();
            }
            
            List<Heartrate> proccessHeartrates = [];
            switch (period)
            {
                case Period.Hour:
                {
                    _logger.LogInformation("Processing current heart rate data for elder: {ElderEmail}", elderEmail);
                    Heartrate heartrate = new Heartrate
                    {
                        Avgrate = (int)currentHeartRateData.Average(h => h.Heartrate),
                        Maxrate = currentHeartRateData.Max(h => h.Heartrate),
                        Minrate = currentHeartRateData.Min(h => h.Heartrate),
                        Timestamp = currentHeartRateData.First().Timestamp
                    };
                    return currentHeartRateData.Select(hr => new PostHeartRate
                    {
                        CurrentHeartRate = new currentHeartRate
                        {
                            Heartrate = hr.Heartrate,
                            Timestamp = hr.Timestamp
                        },
                        Heartrate = new Heartrate
                        {
                            Avgrate = heartrate.Avgrate,
                            Maxrate = heartrate.Maxrate,
                            Minrate = heartrate.Minrate,
                            Timestamp = hr.Timestamp
                        }
                    }).ToList();
                }
                case Period.Day:
                {
                    _logger.LogInformation("Processing daily heart rate data for elder: {ElderEmail}", elderEmail);
                    List<Heartrate> hourlyData = currentHeartRateData
                        .GroupBy(h => h.Timestamp.Hour)
                        .Select(g => new Heartrate
                        {
                            Avgrate = (int)g.Average(h => h.Heartrate),
                            Maxrate = g.Max(h => h.Heartrate),
                            Minrate = g.Min(h => h.Heartrate),
                            Timestamp = g.First().Timestamp.Date.AddHours(g.Key)
                        }).ToList();

                    proccessHeartrates.AddRange(hourlyData);
                    break;
                }
                case Period.Week:
                {
                    _logger.LogInformation("Processing weekly heart rate data for elder: {ElderEmail}", elderEmail);
                    List<Heartrate> dailyData = currentHeartRateData
                        .GroupBy(h => h.Timestamp.Date)
                        .Select(g => new Heartrate
                        {
                            Avgrate = (int)g.Average(h => h.Heartrate),
                            Maxrate = g.Max(h => h.Heartrate),
                            Minrate = g.Min(h => h.Heartrate),
                            Timestamp = g.Key
                        }).ToList();

                    proccessHeartrates.AddRange(dailyData);
                    break;
                }
            }

            _logger.LogInformation("ProcessedData {Count}", proccessHeartrates.Count);
            if (proccessHeartrates.Count != 0)
                return proccessHeartrates.Count != 0
                    ? proccessHeartrates.Select(hr =>
                        new PostHeartRate
                        {
                            CurrentHeartRate = new currentHeartRate
                            {
                                Heartrate = newestHr.Heartrate,
                                Timestamp = hr.Timestamp
                            },
                            Heartrate = new Heartrate
                            {
                                Avgrate = hr.Avgrate,
                                Maxrate = hr.Maxrate,
                                Minrate = hr.Minrate,
                                Timestamp = hr.Timestamp
                            }
                        }).ToList()
                    : [];
            _logger.LogError("No processed heart rate data available for elder: {ElderEmail}", elderEmail);
            return new BadRequestObjectResult("No data available for the specified parameters.");
    }

    public async Task<ActionResult<List<PostSpo2>>> GetSpO2(string elderEmail, DateTime date, Period period)
    {
         // Fetch historical SpO2 data
            List<Spo2> data = await _getHealthDataService.GetHealthData<Spo2>(
                elderEmail, period, date.ToUniversalTime());
            _logger.LogInformation("Fetched historical SpO2 data: {Count}", data.Count);
            // Fetch current SpO2 data if historical data is unavailable
            List<Max30102> currentSpo2Data =
                await _getHealthDataService.GetHealthData<Max30102>(elderEmail, period, date.ToUniversalTime());
            _logger.LogInformation("Fetched current SpO2 data: {Count}", currentSpo2Data.Count);
            if (data.Count != 0)
            {
                _logger.LogInformation("Processing historical SpO2 data for elder: {ElderEmail}", elderEmail);
                return data.Select(spo2 =>
                    new PostSpo2
                    {
                        CurrentSpo2 = new currentSpo2
                        {
                            SpO2 = currentSpo2Data.OrderByDescending(s => s.Timestamp).FirstOrDefault()?.SpO2 ?? 0,
                            Timestamp = spo2.Timestamp
                        },
                        Spo2 = new Spo2
                        {
                            AvgSpO2 = spo2.AvgSpO2,
                            MaxSpO2 = spo2.MaxSpO2,
                            MinSpO2 = spo2.MinSpO2,
                            Timestamp = spo2.Timestamp
                        }
                    }).ToList();
            }

            List<Spo2> processedSpo2 = [];
            switch (period)
            {
                case Period.Hour:
                {
                    _logger.LogInformation("Processing current SpO2 data for elder: {ElderEmail}", elderEmail);
                    Spo2 spo2 = new Spo2
                    {
                        AvgSpO2 = currentSpo2Data.Average(s => s.SpO2),
                        MaxSpO2 = currentSpo2Data.Max(s => s.SpO2),
                        MinSpO2 = currentSpo2Data.Min(s => s.SpO2),
                        Timestamp = currentSpo2Data.First().Timestamp
                    };
                    return currentSpo2Data.Select(s => new PostSpo2
                    {
                        CurrentSpo2= new currentSpo2
                        {
                            SpO2 = s.SpO2,
                            Timestamp = s.Timestamp
                        },
                        Spo2 = new Spo2
                        {
                            AvgSpO2 = spo2.AvgSpO2,
                            MaxSpO2 = spo2.MaxSpO2,
                            MinSpO2 = spo2.MinSpO2,
                            Timestamp = s.Timestamp
                        }
                    }).ToList();
                }
                case Period.Day:
                {
                    _logger.LogInformation("Processing daily SpO2 data for elder: {ElderEmail}", elderEmail);
                    List<Spo2> hourlyData = currentSpo2Data
                        .GroupBy(s => s.Timestamp.Hour)
                        .Select(g => new Spo2
                        {
                            AvgSpO2 = g.Average(s => s.SpO2),
                            MaxSpO2 = g.Max(s => s.SpO2),
                            MinSpO2 = g.Min(s => s.SpO2),
                            Timestamp = g.First().Timestamp.Date.AddHours(g.Key)
                        }).ToList();

                    processedSpo2.AddRange(hourlyData);
                    break;
                }
                case Period.Week:
                {
                    _logger.LogInformation("Processing weekly SpO2 data for elder: {ElderEmail}", elderEmail);
                    List<Spo2> dailyData = currentSpo2Data
                        .GroupBy(s => s.Timestamp.Date)
                        .Select(g => new Spo2
                        {
                            AvgSpO2 = g.Average(s => s.SpO2),
                            MaxSpO2 = g.Max(s => s.SpO2),
                            MinSpO2 = g.Min(s => s.SpO2),
                            Timestamp = g.Key
                        }).ToList();

                    processedSpo2.AddRange(dailyData);
                    break;
                }
            }

            _logger.LogInformation("ProcessedData {Count}", processedSpo2.Count);
            return processedSpo2.Count != 0
                ? processedSpo2.Select(spo2 =>
                    new PostSpo2
                    {
                        CurrentSpo2 = new currentSpo2
                        {
                            SpO2 = currentSpo2Data.OrderByDescending(s => s.Timestamp).FirstOrDefault()?.SpO2 ?? 0,
                            Timestamp = spo2.Timestamp
                        },
                        Spo2 = new Spo2
                        {
                            AvgSpO2 = spo2.AvgSpO2,
                            MaxSpO2 = spo2.MaxSpO2,
                            MinSpO2 = spo2.MinSpO2,
                            Timestamp = spo2.Timestamp
                        }
                    }).ToList()
                : [];
    }
}