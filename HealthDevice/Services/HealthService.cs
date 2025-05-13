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
    private readonly IRepository<Steps> _stepsRepository;
    private readonly IRepository<DistanceInfo> _distanceInfoRepository;
    private readonly IRepository<FallInfo> _fallInfoRepository;
    private readonly IRepository<Heartrate> _heartrateRepository;
    private readonly IRepository<Spo2> _spo2Repository;

    public HealthService(ILogger<HealthService> logger, IRepositoryFactory repositoryFactory,
        IEmailService emailService, IGetHealthData getHealthDataService, ITimeZoneService timeZoneService,
        IRepository<Elder> elderRepository, IRepository<Caregiver> caregiverRepository,
        IRepository<Perimeter> perimeterRepository, IRepository<Location> locationRepository,
        IRepository<Steps> stepsRepository, IRepository<DistanceInfo> distanceInfoRepository,
        IRepository<FallInfo> fallInfoRepository, IRepository<Heartrate> heartrateRepository,
        IRepository<Spo2> spo2Repository)
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
        _stepsRepository = stepsRepository;
        _distanceInfoRepository = distanceInfoRepository;
        _fallInfoRepository = fallInfoRepository;
        _heartrateRepository = heartrateRepository;
        _spo2Repository = spo2Repository;
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

    public async Task DeleteData<T>(DateTime currentDate, string arduino) where T : Sensor
    {
        IRepository<T> repository = _repositoryFactory.GetRepository<T>();
        List<T> data = await repository.Query()
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
        //The Haversine formula to calculate the distance between two points on the earth
        //Link https://www.movable-type.co.uk/scripts/latlong.html
        double dLat = (perimeter.Latitude.Value - location.Latitude) * Math.PI / 180;
        double dLon = (perimeter.Longitude.Value - location.Longitude) * Math.PI / 180;
        double lat1 = location.Latitude * Math.PI / 180;
        double lat2 = perimeter.Latitude.Value * Math.PI / 180;

        int RADIUS_OF_EARTH = 6371;

        double a = Math.Pow(Math.Sin(dLat / 2), 2) +
                   Math.Cos(lat1) * Math.Cos(lat2) *
                   Math.Pow(Math.Sin(dLon / 2), 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        double d = RADIUS_OF_EARTH * c;

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

    public async Task<ActionResult> SetPerimeter(int radius, string elderId)
    {
        if (radius < 0)
            return new BadRequestObjectResult("Invalid radius value.");

        Elder? elder = await _elderRepository.Query().FirstOrDefaultAsync(m => m.Id == elderId);
        if (elder is null || string.IsNullOrEmpty(elder.MacAddress))
            return new BadRequestObjectResult("Elder Arduino not set.");

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

        _logger.LogInformation("Setting perimeter for elder: {ElderEmail}", elder.Name);

        await _emailService.SendEmail(
            "Perimeter set",
            $"Perimeter set for elder {elder.Name} with radius {radius} meters.", elder);

        return new OkObjectResult("Perimeter set successfully");
    }

    public async Task<ActionResult<PerimeterDTO>> GetElderPerimeter(string elderId)
    {
        Elder? elder = await _elderRepository.Query()
            .Include(e => e.Caregiver)
            .FirstOrDefaultAsync(m => m.Id == elderId);
        if (elder == null)
            return new BadRequestObjectResult("Elder not found.");

        if (string.IsNullOrEmpty(elder.MacAddress))
            return new BadRequestObjectResult("Elder Arduino not set.");

        Location? location = await _locationRepository.Query()
            .FirstOrDefaultAsync(m => m.MacAddress == elder.MacAddress);
        if (location == null)
            return new BadRequestObjectResult("Location not found.");

        Perimeter? perimeter = await _perimeterRepository.Query()
            .FirstOrDefaultAsync(m => m.MacAddress == elder.MacAddress);

        _logger.LogInformation("Fetched location data for elder: {ElderEmail}", elder.Email);
        return new PerimeterDTO
        {
            HomeLatitude = elder.Latitude,
            HomeLongitude = elder.Longitude,
            HomeRadius = perimeter?.Radius ?? 10
        };
    }

    public async Task<ActionResult<List<ElderLocationDTO>>> GetEldersLocation(string caregiverId)
    {
        Caregiver? caregiver = await _caregiverRepository.Query()
            .Include(c => c.Elders)
            .FirstOrDefaultAsync(c => c.Id == caregiverId);
        if (caregiver == null)
            return new BadRequestObjectResult("Caregiver not found.");

        List<Elder>? elders = caregiver.Elders;
        if (elders == null || elders.Count == 0)
            return new BadRequestObjectResult("No elders found for the caregiver.");

        _logger.LogInformation("Found {ElderCount} elders for caregiver: {CaregiverEmail}", elders.Count,
            caregiver.Email);
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

    public async Task<ActionResult<List<FallDTO>>> GetFalls(string elderId, DateTime date, Period period,
        TimeZoneInfo timezone)
    {
        DateTime endTime = period.GetEndDate(date);
        List<FallInfo> data = await _getHealthDataService.GetHealthData<FallInfo>(
            elderId, period, endTime, timezone);

        List<FallDTO> result = PeriodUtil.AggregateByPeriod(
            data,
            period,
            date,
            timezone,
            _timeZoneService,
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
        _logger.LogInformation("Fetched fall data: {Count}", result.Count);
        return result;
    }


    public async Task<ActionResult<List<StepsDTO>>> GetSteps(string elderId, DateTime date, Period period,
        TimeZoneInfo timezone)
    {
        DateTime endTime = period.GetEndDate(date);
        List<Steps> data = await _getHealthDataService.GetHealthData<Steps>(elderId, period, endTime, timezone);

        List<StepsDTO> result = PeriodUtil.AggregateByPeriod(
            data,
            period,
            date,
            timezone,
            _timeZoneService,
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
        _logger.LogInformation("Fetched step data: {Count}", result.Count);
        return result;
    }


    public async Task<ActionResult<List<DistanceInfoDTO>>> GetDistance(string elderId, DateTime date, Period period,
        TimeZoneInfo timezone)
    {
        DateTime endTime = period.GetEndDate(date);
        List<DistanceInfo> data = await _getHealthDataService.GetHealthData<DistanceInfo>(
            elderId, period, endTime, timezone);

        List<DistanceInfoDTO> result = PeriodUtil.AggregateByPeriod(
            data,
            period,
            date,
            timezone,
            _timeZoneService,
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
        _logger.LogInformation("Fetched distance data: {Count}", result.Count);
        return result;
    }


    public async Task<ActionResult<List<PostHeartRate>>> GetHeartrate(string elderId, DateTime date, Period period,
        TimeZoneInfo timezone)
    {
        DateTime endTime = period.GetEndDate(date);

        List<Heartrate> data = await _getHealthDataService.GetHealthData<Heartrate>(
            elderId, period, endTime, timezone);

        return PeriodUtil.AggregateByPeriod(
            data,
            period,
            date,
            timezone,
            _timeZoneService,
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
                };
            },
            slot => new PostHeartRate
            {
                Avgrate = 0,
                Maxrate = 0,
                Minrate = 0,
                Timestamp = _timeZoneService.UTCToLocalTime(timezone, slot),
            }
        );
    }

    public async Task<ActionResult<List<PostSpO2>>> GetSpO2(string elderId, DateTime date, Period period,
        TimeZoneInfo timezone)
    {
        DateTime endTime = period.GetEndDate(date);

        List<Spo2> data = await _getHealthDataService.GetHealthData<Spo2>(
            elderId, period, endTime, timezone);

        return PeriodUtil.AggregateByPeriod(
            data,
            period,
            date,
            timezone,
            _timeZoneService,
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
                };
            },
            slot => new PostSpO2
            {
                AvgSpO2 = 0,
                MaxSpO2 = 0,
                MinSpO2 = 0,
                Timestamp = _timeZoneService.UTCToLocalTime(timezone, slot),
            }
        );
    }

    public async Task<ActionResult<DashBoard>> GetDashboardData(string macAddress)
    {
        DateTime currentDate = DateTime.UtcNow;

        Heartrate? heartrate = await _heartrateRepository.Query()
            .Where(s => s.MacAddress == macAddress && s.Timestamp.Date == currentDate.Date)
            .OrderByDescending(s => s.Timestamp)
            .FirstOrDefaultAsync();

        Spo2? spo2 = await _spo2Repository.Query()
            .Where(s => s.MacAddress == macAddress && s.Timestamp.Date == currentDate.Date)
            .OrderByDescending(s => s.Timestamp)
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

        return new DashBoard
        {
            FallCount = _fallInfoRepository.Query().Where(t => t.Timestamp.Date == currentDate.Date)
                .Count(f => f.MacAddress == macAddress),
            Distance = kilometer?.Distance ?? 0,
            HeartRate = heartrate?.Lastrate ?? 0,
            SpO2 = spo2?.LastSpO2 ?? 0,
            Steps = steps?.StepsCount ?? 0
        };
    }
}