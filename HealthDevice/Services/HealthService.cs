using HealthDevice.DTO;
using HealthDevice.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StepsDTO = HealthDevice.DTO.StepsDTO;
// ReSharper disable SuggestVarOrType_SimpleTypes

namespace HealthDevice.Services;

public class HealthService : IHealthService
{
    private readonly ILogger<HealthService> _logger;
    private readonly IEmailService _emailService;
    private readonly IRepositoryFactory _repositoryFactory;
    private readonly IGetHealthData _getHealthDataService;
    private readonly ITimeZoneService _timeZoneService;
    private readonly IRepository<Elder> _elderRepository;
    private readonly IRepository<Caregiver> _caregiverRepository;
    private readonly IRepository<Perimeter> _perimeterRepository;
    private readonly IRepository<Location> _locationRepository;
    private readonly IRepository<Max30102> _max30102Repository;
    private readonly IRepository<Steps> _stepsRepository;
    private readonly IRepository<DistanceInfo> _distanceInfoRepository;
    private readonly IRepository<FallInfo> _fallInfoRepository;

    public HealthService(ILogger<HealthService> logger, IRepositoryFactory repositoryFactory,
        IEmailService emailService, IGetHealthData getHealthDataService, ITimeZoneService timeZoneService,
        IRepository<Elder> elderRepository, IRepository<Caregiver> caregiverRepository,
        IRepository<Perimeter> perimeterRepository, IRepository<Location> locationRepository,
        IRepository<Max30102> max30102Repository,
        IRepository<Steps> stepsRepository, IRepository<DistanceInfo> distanceInfoRepository,
        IRepository<FallInfo> fallInfoRepository)
    {
        _logger = logger;
        _repositoryFactory = repositoryFactory;
        _emailService = emailService;
        _getHealthDataService = getHealthDataService;
        _timeZoneService = timeZoneService;
        _elderRepository = elderRepository;
        _caregiverRepository = caregiverRepository;
        _perimeterRepository = perimeterRepository;
        _locationRepository = locationRepository;
        _max30102Repository = max30102Repository;
        _stepsRepository = stepsRepository;
        _distanceInfoRepository = distanceInfoRepository;
        _fallInfoRepository = fallInfoRepository;
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
            return [];
        }

        DateTime earliestDate = heartRates.Min(h => h.Timestamp);
        List<Heartrate> heartRateList = [];

        for (DateTime date = earliestDate; date <= currentDate; date = date.AddHours(1))
        {
            var date1 = date;
            IEnumerable<Max30102> heartRateInHour = heartRates.Where(h => h.Timestamp >= date1 && h.Timestamp < date1.AddHours(1));
            List<Max30102> rateInHour = heartRateInHour.ToList();
            if (rateInHour.Count == 0) continue;
            heartRateList.Add(new Heartrate
            {
                Avgrate = (int)rateInHour.Average(hr => hr.AvgHeartrate),
                Maxrate = rateInHour.Max(hr => hr.MaxHeartrate),
                Minrate = rateInHour.Min(hr => hr.MinHeartrate),
                Timestamp = new DateTime(date.Year, date.Month, date.Day, date.Hour, 0, 0).ToUniversalTime(),
                MacAddress = address
            });
        }
        _logger.LogInformation("Found {Count} heart rate records for elder with MacAddress {Address}", heartRateList.Count, address);
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
            return [];
        }

        DateTime earliestDate = spo2Data.Min(s => s.Timestamp);
        List<Spo2> spo2List = [];

        for (DateTime date = earliestDate; date <= currentDate; date = date.AddHours(1))
        {
            List<Max30102> hourlyData = spo2Data.Where(s => s.Timestamp >= date && s.Timestamp < date.AddHours(1)).ToList();
            if (hourlyData.Count == 0) continue;
            
            spo2List.Add(new Spo2
            {
                AvgSpO2 = hourlyData.Average(sp => sp.AvgSpO2),
                MaxSpO2 = hourlyData.Max(sp => sp.MaxSpO2),
                MinSpO2 = hourlyData.Min(sp => sp.MinSpO2),
                Timestamp = new DateTime(date.Year, date.Month, date.Day, date.Hour, 0, 0).ToUniversalTime(),
                MacAddress = address
            });
        }
        _logger.LogInformation("Found {Count} SpO2 records for elder with MacAddress {Address}", spo2List.Count, address);
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
            _logger.LogWarning("Not enough GPS data to calculate Distance for elder {Arduino}", arduino);
            return new DistanceInfo
            {
                MacAddress = String.Empty
            };
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
            _logger.LogWarning("No Distance data found for elder {Arduino}", arduino);
            return new DistanceInfo
            {
                MacAddress = string.Empty
            };
        }

        _logger.LogInformation("Distance data found for elder {Arduino}", arduino);
        return new DistanceInfo
        {
            Distance = distance,
            Timestamp = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, currentDate.Hour,
                currentDate.Minute, 0),
            MacAddress = arduino
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

    public async Task ComputeOutOfPerimeter(string arduino, Location location)
    {
        Perimeter? perimeter = await _perimeterRepository.Query()
            .FirstOrDefaultAsync(p => p.MacAddress == arduino);
        if (perimeter == null)
        {
            _logger.LogWarning("No perimeter found for elder with arduino {arduino}", arduino);
            return;
        }

        Elder? elder = await _elderRepository.Query()
            .FirstOrDefaultAsync(e => e.MacAddress == arduino);
        if (elder == null)
        {
            _logger.LogWarning("Elder with arduino {arduino} not found", arduino);
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
        
        if (elder.OutOfPerimeter)
        {
            if (d < perimeter.Radius)
            {
                elder.OutOfPerimeter = false;
                await _elderRepository.Update(elder);
                _logger.LogInformation("Elder {Email} is back in perimeter", elder.Email);
                return;
            }

            _logger.LogInformation("Elder {Email} is already out of perimeter", elder.Email);
            return;
        }

        if (d > perimeter.Radius)
        {
            _logger.LogInformation("Elder {Email} is out of perimeter", elder.Email);
            elder.OutOfPerimeter = true;
            await _elderRepository.Update(elder);
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
        Elder? elder = await _elderRepository.Query().FirstOrDefaultAsync(m => m.Email == elderEmail);
        if (elder is null || string.IsNullOrEmpty(elder.MacAddress))
            return new BadRequestObjectResult("Elder Arduino not set.");

        if (radius < 0)
            return new BadRequestObjectResult("Invalid radius value.");

        Perimeter? oldPerimeter = await _perimeterRepository.Query()
            .OrderByDescending(i => i.Id)
            .FirstOrDefaultAsync(m => m.MacAddress == elder.MacAddress);

        if (oldPerimeter == null)
        {
            Perimeter perimeter = new Perimeter
            {
                Latitude = elder.Latitude,
                Longitude = elder.Longitude,
                Radius = radius,
                MacAddress = elder.MacAddress
            };
            await _perimeterRepository.Add(perimeter);
        }
        else
        {
            oldPerimeter.Latitude = elder.Latitude;
            oldPerimeter.Longitude = elder.Longitude;
            oldPerimeter.Radius = radius;

            await _perimeterRepository.Update(oldPerimeter);
        }
        _logger.LogInformation("Setting perimeter for elder: {ElderEmail}", elderEmail);
        
        await _emailService.SendEmail(
            "Perimeter set",
            $"Perimeter set for elder {elder.Name} with radius {radius} meters.", elder);

        return new OkObjectResult("Perimeter set successfully");
    }

    public async Task<ActionResult<List<ElderLocationDTO>>> GetEldersLocation(string email)
    {
        Caregiver? caregiver = await _caregiverRepository.Query()
            .Include(c => c.Elders)
            .FirstOrDefaultAsync(c => c.Email == email);
        if (caregiver == null)
            return new BadRequestObjectResult("Caregiver not found.");

        List<Elder>? elders = caregiver.Elders;
        if (elders == null || elders.Count == 0)
            return new BadRequestObjectResult("No elders found for the caregiver.");

        _logger.LogInformation("Found {ElderCount} elders for caregiver: {CaregiverEmail}", elders.Count, email);
        List<ElderLocationDTO> elderLocations = [];
        foreach (Elder elder in elders)
        {
            if (string.IsNullOrEmpty(elder.MacAddress))
            {
                _logger.LogError("Elder Arduino not set for elder: {ElderEmail}", elder.Email);
                continue;
            }
            
            Location? location =
                await _locationRepository.Query().FirstOrDefaultAsync(m => m.MacAddress == elder.MacAddress);
            if (location == null || elder.Email == null) continue;
            Perimeter? perimeter = await _perimeterRepository.Query()
                .FirstOrDefaultAsync(m => m.MacAddress == elder.MacAddress);
            elderLocations.Add(new ElderLocationDTO
            {
                Email = elder.Email,
                Name = elder.Name,
                Latitude = location.Latitude,
                Longitude = location.Longitude,
                LastUpdated = location.Timestamp,
                Perimeter = new PerimeterDTO
                {
                    HomeLatitude = elder.Latitude,
                    HomeLongitude = elder.Longitude,
                    HomeRadius = perimeter?.Radius ?? 10
                }
            });
        }

        _logger.LogInformation("Found {LocationCount} locations for the elders.", elderLocations.Count);
        return elderLocations;
    }

    private List<PostHeartRate> GetHeartrateFallback(List<Heartrate> data,
       List<Max30102> Max30102Data, Period period, TimeZoneInfo timezone, DateTime endTime)
    {
        if (Max30102Data.Count == 0)
        {
            return [];
        }

        // Process current heart rate data based on the period
        List<PostHeartRate> processedHeartrates = [];
        switch (period)
        {
            case Period.Hour:
                processedHeartrates.AddRange(Max30102Data.Select(g => new PostHeartRate
                {
                    Avgrate = g.AvgHeartrate,
                    Maxrate = g.MaxHeartrate,
                    Minrate = g.MinHeartrate,
                    Timestamp = _timeZoneService.UTCToLocalTime(timezone, g.Timestamp),
                    MacAddress = g.MacAddress
                }));
                return processedHeartrates;
            case Period.Day:
                processedHeartrates.AddRange(Max30102Data
                    .GroupBy(h => h.Timestamp.Hour)
                    .Select(g => new PostHeartRate()
                    {
                        Avgrate = (int)g.Average(h => h.AvgHeartrate),
                        Maxrate = g.Max(h => h.MaxHeartrate),
                        Minrate = g.Min(h => h.MinHeartrate),
                        Timestamp = _timeZoneService.UTCToLocalTime(timezone, endTime.Date.AddHours(g.Key)),
                        MacAddress = g.First().MacAddress
                    }));
                return processedHeartrates;
            case Period.Week:
                processedHeartrates.AddRange(Max30102Data
                    .GroupBy(h => h.Timestamp.Date)
                    .Select(g => new PostHeartRate()
                    {
                        Avgrate = (int)g.Average(h => h.AvgHeartrate),
                        Maxrate = g.Max(h => h.MaxHeartrate),
                        Minrate = g.Min(h => h.MinHeartrate),
                        Timestamp = _timeZoneService.UTCToLocalTime(timezone, g.Key),
                        MacAddress = g.First().MacAddress
                    }));
                DateTime startDate = endTime.Date.AddDays(-6);
                DateTime endDate = endTime.Date;

                for (DateTime currentDate = startDate; currentDate <= endDate; currentDate = currentDate.AddDays(1))
                {
                    if (processedHeartrates.Any(hr => hr.Timestamp.Date == currentDate.Date)) continue;
                    List<Heartrate> fallbackData = data.Where(d => d.Timestamp.Date == currentDate.Date).ToList();
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
                return processedHeartrates.Where(t => t.Timestamp.Date <= endDate.Date).ToList();
            default:
                return [];
        }
    }

    private List<PostSpO2> GetSpO2FallBack(List<Spo2> data,
       List<Max30102> Max30102Data, Period period, TimeZoneInfo timezone, DateTime endTime)
    {
        if (Max30102Data.Count == 0)
        {
            return [];
        }

        // Process current SpO2 data based on the period
        List<PostSpO2> processedSpo2 = [];
        switch (period)
        {
            case Period.Hour:
                processedSpo2.AddRange(Max30102Data.Select(g => new PostSpO2
                {
                    AvgSpO2 = g.AvgSpO2,
                    MaxSpO2 = g.MaxSpO2,
                    MinSpO2 = g.MinSpO2,
                    Timestamp = _timeZoneService.UTCToLocalTime(timezone, g.Timestamp),
                    MacAddress = g.MacAddress
                }));
                return processedSpo2;
            case Period.Day:
                processedSpo2.AddRange(Max30102Data
                    .GroupBy(s => s.Timestamp.Hour)
                    .Select(g => new PostSpO2
                    {
                        AvgSpO2 = g.Average(s => s.AvgSpO2),
                        MaxSpO2 = g.Max(s => s.MaxSpO2),
                        MinSpO2 = g.Min(s => s.MinSpO2),
                        Timestamp = _timeZoneService.UTCToLocalTime(timezone, endTime.Date.AddHours(g.Key)),
                        MacAddress = g.First().MacAddress
                    }));
                return processedSpo2;
            case Period.Week:
                processedSpo2.AddRange(Max30102Data
                    .GroupBy(s => s.Timestamp.Date)
                    .Select(g => new PostSpO2
                    {
                        AvgSpO2 = g.Average(s => s.AvgSpO2),
                        MaxSpO2 = g.Max(s => s.MaxSpO2),
                        MinSpO2 = g.Min(s => s.MinSpO2),
                        Timestamp = _timeZoneService.UTCToLocalTime(timezone, g.Key),
                        MacAddress = g.First().MacAddress
                    }));

                DateTime startDate = endTime.Date.AddDays(-6);
                DateTime endDate = endTime.Date;

                for (DateTime currentDate = startDate; currentDate <= endDate; currentDate = currentDate.AddDays(1))
                {
                    if (processedSpo2.Any(hr => hr.Timestamp.Date == currentDate.Date)) continue;
                    List<Spo2> fallbackData = data.Where(d => d.Timestamp.Date == currentDate.Date).ToList();
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

                return processedSpo2.Where(t => t.Timestamp.Date <= endDate.Date).ToList();
            default:
                return [];
        }
    }

    public async Task<ActionResult<List<FallDTO>>> GetFalls(string elderEmail, DateTime date, Period period,
      TimeZoneInfo timezone)
    {
        DateTime endTime = period.GetEndDate(date);
        List<FallInfo> data = await _getHealthDataService.GetHealthData<FallInfo>(
            elderEmail, period, endTime, timezone);
        
        List<FallDTO> result = PeriodUtil.AggregateByPeriod(
            data,
            period,
            date,
            x => x.Timestamp,
            (group, slot) => new FallDTO
            {
                Timestamp = _timeZoneService.UTCToLocalTime(timezone, slot),
                FallCount = group.Count()
            },
            slot => new FallDTO
            {
                Timestamp = _timeZoneService.UTCToLocalTime(timezone, slot),
                FallCount = 0
            }
        );
        _logger.LogInformation("Fetched fall data: {Count}, for Elder {elder}", result.Count, elderEmail);
        return result;

    }


    public async Task<ActionResult<List<StepsDTO>>> GetSteps(string elderEmail, DateTime date, Period period, TimeZoneInfo timezone)
    {
        DateTime endTime = period.GetEndDate(date);
        List<Steps> data = await _getHealthDataService.GetHealthData<Steps>(elderEmail, period, endTime, timezone);

        List<StepsDTO> result = PeriodUtil.AggregateByPeriod(
            data,
            period,
            date,
            x => x.Timestamp,
            (group, slot) => new StepsDTO
            {
                Timestamp = _timeZoneService.UTCToLocalTime(timezone, slot),
                StepsCount = group.Sum(s => s.StepsCount)
            },
            slot => new StepsDTO
            {
                Timestamp = _timeZoneService.UTCToLocalTime(timezone, slot),
                StepsCount = 0
            }
        );
        _logger.LogInformation("Fetched step data: {Count}, for Elder {elder}", result.Count, elderEmail);
        return result;
    }


    public async Task<ActionResult<List<DistanceInfoDTO>>> GetDistance(string elderEmail, DateTime date, Period period,
        TimeZoneInfo timezone)
    {
        DateTime endTime = period.GetEndDate(date);
        List<DistanceInfo> data = await _getHealthDataService.GetHealthData<DistanceInfo>(
            elderEmail, period, endTime, timezone);

        List<DistanceInfoDTO> result = PeriodUtil.AggregateByPeriod(
            data,
            period,
            date,
            x => x.Timestamp,
            (group, slot) => new DistanceInfoDTO
            {
                Timestamp = _timeZoneService.UTCToLocalTime(timezone, slot),
                Distance = group.Sum(s => s.Distance)
            },
            slot => new DistanceInfoDTO
            {
                Timestamp = _timeZoneService.UTCToLocalTime(timezone, slot),
                Distance = 0
            }
        );
        _logger.LogInformation("Fetched distance data: {Count}, for Elder {elder}", result.Count, elderEmail);
        return result;

    }


    public async Task<ActionResult<List<PostHeartRate>>> GetHeartrate(string elderEmail, DateTime date, Period period,
        TimeZoneInfo timezone)
    {
        DateTime endTime = period.GetEndDate(date);

        List<Heartrate> data = await _getHealthDataService.GetHealthData<Heartrate>(
            elderEmail, period, endTime, timezone);
        
        List<Max30102> Max30102Data =
            await _getHealthDataService.GetHealthData<Max30102>(elderEmail, period, endTime, timezone);

        if (data.Count != 0 && Max30102Data.Count < 7)
        {
            _logger.LogInformation("Processing historical heart rate data for elder: {ElderEmail}", elderEmail);
            return PeriodUtil.AggregateByPeriod(
                data,
                period,
                date,
                x => x.Timestamp,
                (group, slot) =>
                {
                    IEnumerable<Heartrate> heartrates = group.ToList();
                    return new PostHeartRate
                    {
                        Avgrate = (int)heartrates.Average(h => h.Avgrate),
                        Maxrate = heartrates.Max(h => h.Maxrate),
                        Minrate = heartrates.Min(h => h.Minrate),
                        Timestamp = _timeZoneService.UTCToLocalTime(timezone, slot),
                        MacAddress = heartrates.First().MacAddress
                    };
                },
                slot => new PostHeartRate
                {
                    Avgrate = 0,
                    Maxrate = 0,
                    Minrate = 0,
                    Timestamp = _timeZoneService.UTCToLocalTime(timezone, slot),
                    MacAddress = string.Empty
                }
            );
        }

        List<PostHeartRate> processedHeartrates = GetHeartrateFallback(data, Max30102Data, period, timezone, endTime);

        _logger.LogInformation("Fetched Heartrate data: {Count}, for Elder {elder}", processedHeartrates.Count, elderEmail);
        return processedHeartrates.OrderBy(t => t.Timestamp).ToList();
    }

    public async Task<ActionResult<List<PostSpO2>>> GetSpO2(string elderEmail, DateTime date, Period period,
        TimeZoneInfo timezone)
    {
        DateTime endTime = period.GetEndDate(date);

        List<Spo2> data = await _getHealthDataService.GetHealthData<Spo2>(
            elderEmail, period, endTime, timezone);
        
        List<Max30102> Max30102Data =
            await _getHealthDataService.GetHealthData<Max30102>(elderEmail, period, endTime, timezone);

        if (data.Count != 0 && Max30102Data.Count < 7)
        {
            _logger.LogInformation("Processing historical SpO2 data for elder: {ElderEmail}", elderEmail);
            return PeriodUtil.AggregateByPeriod(
                data,
                period,
                date,
                x => x.Timestamp,
                (group, slot) =>
                {
                    IEnumerable<Spo2> enumerable = group.ToList();
                    return new PostSpO2
                    {
                        AvgSpO2 = enumerable.Average(h => h.AvgSpO2),
                        MaxSpO2 = enumerable.Max(h => h.MaxSpO2),
                        MinSpO2 = enumerable.Min(h => h.MinSpO2),
                        Timestamp = _timeZoneService.UTCToLocalTime(timezone, slot),
                        MacAddress = enumerable.First().MacAddress
                    };
                },
                slot => new PostSpO2
                {
                    AvgSpO2 = 0,
                    MaxSpO2 = 0,
                    MinSpO2 = 0,
                    Timestamp = _timeZoneService.UTCToLocalTime(timezone, slot),
                    MacAddress = string.Empty
                }
            );
        }
        List<PostSpO2> processedSpo2 = GetSpO2FallBack(data, Max30102Data, period, timezone, endTime);
        _logger.LogInformation("Fetched SpO2 data: {Count}, for Elder {elder}", processedSpo2.Count, elderEmail);
        return processedSpo2.OrderBy(t => t.Timestamp).ToList();
    }

    public async Task<ActionResult<DashBoard>> GetDashboardData(string macAddress, Elder elder)
    {
        DateTime currentDate = DateTime.UtcNow;

        Max30102? max30102 = await _max30102Repository.Query()
            .Where(m => m.MacAddress == macAddress && m.Timestamp.Date == currentDate.Date)
            .OrderByDescending(m => m.Timestamp)
            .FirstOrDefaultAsync();
        
        DistanceInfo? kilometer = await _distanceInfoRepository.Query()
            .Where(s => s.MacAddress == macAddress && s.Timestamp.Date == currentDate.Date)
            .GroupBy(s => s.Timestamp.Date)
            .Select(g => new DistanceInfo
            {
                Distance = g.Sum(s => s.Distance),
                Timestamp = g.Key,
                MacAddress = macAddress
            }).FirstOrDefaultAsync();

        Steps? steps = await _stepsRepository.Query()
            .Where(s => s.MacAddress == macAddress && s.Timestamp.Date == currentDate.Date)
            .GroupBy(s => s.Timestamp.Date)
            .Select(g => new Steps
            {
                StepsCount = g.Sum(s => s.StepsCount),
                Timestamp = g.Key,
                MacAddress = macAddress
            }).FirstOrDefaultAsync();

        _logger.LogInformation("Fetched DashBoard for elder: {ElderEmail}", elder.Email);

        return new DashBoard
        {
            FallCount = _fallInfoRepository.Query().Where(t => t.Timestamp.Date == currentDate.Date)
                .Count(f => f.MacAddress == macAddress),
            Distance = kilometer?.Distance ?? 0,
            HeartRate = max30102?.LastHeartrate ?? 0,
            SpO2 = max30102?.LastSpO2 ?? 0,
            Steps = steps?.StepsCount ?? 0
        };
    }
}