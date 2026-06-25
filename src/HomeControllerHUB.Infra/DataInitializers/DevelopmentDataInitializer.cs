using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Infra.DatabaseContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace HomeControllerHUB.Infra.DataInitializers;

public class DevelopmentDataInitializer : BaseDataInitializer, IDataInitializer
{
    private const string DemoDeviceIdPrefix = "demo-";
    private const string DemoEstablishmentName = "WillHome Demo";

    private readonly ApplicationDbContext _context;
    private readonly IHostEnvironment _environment;

    public DevelopmentDataInitializer(ApplicationDbContext context, IHostEnvironment environment) : base(8)
    {
        _context = context;
        _environment = environment;
    }

    public override void InitializeData()
    {
        if (!_environment.IsDevelopment())
        {
            return;
        }

        if (_context.Sensors.AsNoTracking().Any(sensor => sensor.DeviceId.StartsWith(DemoDeviceIdPrefix)))
        {
            return;
        }

        var now = DateTime.UtcNow;
        var establishment = GetOrCreateEstablishment();
        var locations = CreateLocations(establishment.Id);
        var sensors = CreateSensors(establishment.Id, locations, now);

        _context.Sensors.AddRange(sensors.Values);
        _context.SaveChanges();

        var alerts = CreateAlerts(sensors, now);

        _context.SensorReadings.AddRange(CreateReadings(sensors, now));
        _context.SensorStatusUpdates.AddRange(CreateStatusUpdates(sensors, now));
        _context.SensorAlerts.AddRange(alerts);
        _context.SaveChanges();

        foreach (var alert in alerts)
        {
            alert.Created = alert.Timestamp;
        }

        _context.SaveChanges();
    }

    public override void ClearData()
    {
        var demoSensors = _context.Sensors
            .Where(sensor => sensor.DeviceId.StartsWith(DemoDeviceIdPrefix))
            .ToList();

        if (demoSensors.Count == 0)
        {
            return;
        }

        var demoSensorIds = demoSensors.Select(sensor => sensor.Id).ToList();

        _context.SensorAlerts.RemoveRange(_context.SensorAlerts.Where(alert => demoSensorIds.Contains(alert.SensorId)));
        _context.SensorReadings.RemoveRange(_context.SensorReadings.Where(reading => demoSensorIds.Contains(reading.SensorId)));
        _context.SensorStatusUpdates.RemoveRange(_context.SensorStatusUpdates.Where(update => demoSensorIds.Contains(update.SensorId)));
        _context.Sensors.RemoveRange(demoSensors);
        _context.SaveChanges();
    }

    private Establishment GetOrCreateEstablishment()
    {
        var establishment = _context.Establishments.FirstOrDefault(establishment => establishment.Name == "WillHome");

        if (establishment is not null)
        {
            return establishment;
        }

        establishment = _context.Establishments.FirstOrDefault(establishment => establishment.Enable && !establishment.IsMaster);

        if (establishment is not null)
        {
            return establishment;
        }

        establishment = new Establishment
        {
            Document = "demo",
            Name = DemoEstablishmentName,
            SiteName = "WillHome Demo",
            Enable = true,
            IsMaster = false
        };

        _context.Establishments.Add(establishment);
        _context.SaveChanges();

        return establishment;
    }

    private Dictionary<string, Location> CreateLocations(Guid establishmentId)
    {
        var casa = GetOrCreateLocation(establishmentId, "Casa", "Residencia principal", LocationType.Building, null);
        var terreo = GetOrCreateLocation(establishmentId, "Terreo", "Pavimento terreo", LocationType.Floor, casa.Id);
        var segundoAndar = GetOrCreateLocation(establishmentId, "Segundo Andar", "Pavimento superior", LocationType.Floor, casa.Id);
        var areaExterna = GetOrCreateLocation(establishmentId, "Area Externa", "Ambientes externos da casa", LocationType.Area, casa.Id);

        var locations = new Dictionary<string, Location>
        {
            ["casa"] = casa,
            ["terreo"] = terreo,
            ["sala"] = GetOrCreateLocation(establishmentId, "Sala de Estar", "Ambiente social monitorado", LocationType.Room, terreo.Id),
            ["cozinha"] = GetOrCreateLocation(establishmentId, "Cozinha", "Area de preparo de alimentos", LocationType.Room, terreo.Id),
            ["garagem"] = GetOrCreateLocation(establishmentId, "Garagem", "Garagem residencial", LocationType.Room, terreo.Id),
            ["segundo-andar"] = segundoAndar,
            ["quarto"] = GetOrCreateLocation(establishmentId, "Quarto", "Quarto principal", LocationType.Room, segundoAndar.Id),
            ["escritorio"] = GetOrCreateLocation(establishmentId, "Escritorio", "Escritorio residencial", LocationType.Room, segundoAndar.Id),
            ["area-externa"] = areaExterna,
            ["jardim"] = GetOrCreateLocation(establishmentId, "Jardim", "Area verde externa", LocationType.Area, areaExterna.Id)
        };

        _context.SaveChanges();

        return locations;
    }

    private Location GetOrCreateLocation(Guid establishmentId, string name, string description, LocationType type, Guid? parentLocationId)
    {
        var existingLocation = _context.Locations.FirstOrDefault(location =>
            location.EstablishmentId == establishmentId &&
            location.Name == name &&
            location.ParentLocationId == parentLocationId);

        if (existingLocation is not null)
        {
            return existingLocation;
        }

        var location = new Location
        {
            EstablishmentId = establishmentId,
            Name = name,
            Description = description,
            Type = type,
            ParentLocationId = parentLocationId
        };

        _context.Locations.Add(location);

        return location;
    }

    private static Dictionary<string, Sensor> CreateSensors(
        Guid establishmentId,
        IReadOnlyDictionary<string, Location> locations,
        DateTime now)
    {
        return new Dictionary<string, Sensor>
        {
            ["temperature"] = CreateSensor(establishmentId, locations["sala"].Id, "Temperatura da Sala", "demo-temp-living-room", SensorType.Temperature, "DHT22", "1.3.2", true, 87, now.AddMinutes(-6), 18, 30),
            ["humidity"] = CreateSensor(establishmentId, locations["quarto"].Id, "Umidade do Quarto", "demo-humidity-bedroom", SensorType.Humidity, "Aqara TH-S2", "2.1.0", true, 52, now.AddMinutes(-18), 35, 75),
            ["door"] = CreateSensor(establishmentId, locations["garagem"].Id, "Porta da Garagem", "demo-door-garage", SensorType.Door, "Sonoff DW2", "1.0.8", true, 78, now.AddMinutes(-4), 0, 1),
            ["smoke"] = CreateSensor(establishmentId, locations["cozinha"].Id, "Fumaca da Cozinha", "demo-smoke-kitchen", SensorType.Smoke, "Nest Protect", "4.0.1", true, 66, now.AddMinutes(-2), 0, 40),
            ["motion"] = CreateSensor(establishmentId, locations["escritorio"].Id, "Movimento do Escritorio", "demo-motion-office", SensorType.Motion, "Philips Hue Motion", "1.8.5", false, 34, now.AddDays(-2).AddHours(-3), 0, 1),
            ["gas"] = CreateSensor(establishmentId, locations["cozinha"].Id, "Gas da Cozinha", "demo-gas-kitchen", SensorType.Gas, "MQ-2", "1.1.4", true, 71, now.AddMinutes(-11), 0, 600),
            ["electricity"] = CreateSensor(establishmentId, locations["garagem"].Id, "Energia da Garagem", "demo-electricity-garage", SensorType.Electricity, "Shelly EM", "2.2.7", true, null, now.AddMinutes(-9), 0, 500),
            ["water"] = CreateSensor(establishmentId, locations["jardim"].Id, "Agua do Jardim", "demo-water-garden", SensorType.Water, "Aqara Water Leak", "1.5.3", true, 11, now.AddMinutes(-7), 0, 1)
        };
    }

    private static Sensor CreateSensor(
        Guid establishmentId,
        Guid locationId,
        string name,
        string deviceId,
        SensorType type,
        string model,
        string firmwareVersion,
        bool isActive,
        double? batteryLevel,
        DateTime lastCommunication,
        double minThreshold,
        double maxThreshold)
    {
        return new Sensor
        {
            EstablishmentId = establishmentId,
            LocationId = locationId,
            Name = name,
            DeviceId = deviceId,
            Type = type,
            Model = model,
            FirmwareVersion = firmwareVersion,
            ApiKey = $"demo-api-key-{deviceId[5..]}",
            MinThreshold = minThreshold,
            MaxThreshold = maxThreshold,
            IsActive = isActive,
            LastCommunication = lastCommunication,
            BatteryLevel = batteryLevel
        };
    }

    private static List<SensorReading> CreateReadings(IReadOnlyDictionary<string, Sensor> sensors, DateTime now)
    {
        var readings = new List<SensorReading>();

        readings.AddRange(CreateSeries(sensors["temperature"], "C", now, [21.8, 22.4, 23.1, 24.7, 26.2, 29.8, 31.4], [23, 12, 6, 2, 1, 0.5, 0.1]));
        readings.AddRange(CreateSeries(sensors["humidity"], "%", now, [49, 53, 58, 62, 67, 71], [25, 18, 9, 4, 1.5, 0.2]));
        readings.AddRange(CreateSeries(sensors["door"], "state", now, [0, 0, 1, 1, 0], [30, 22, 8, 3, 0.1]));
        readings.AddRange(CreateSeries(sensors["smoke"], "ppm", now, [8, 9, 12, 18, 46, 51], [27, 20, 7, 2, 0.4, 0.1]));
        readings.AddRange(CreateSeries(sensors["gas"], "ppm", now, [120, 132, 158, 420, 650, 180], [50, 23, 6, 3, 2, 0.3]));
        readings.AddRange(CreateSeries(sensors["electricity"], "W", now, [118, 146, 210, 286, 342, 401, 255], [49, 23, 10, 5, 2, 0.4, 0.1]));
        readings.AddRange(CreateSeries(sensors["water"], "state", now, [0, 0, 0, 1, 1, 0], [52, 24, 10, 4, 1, 0.1]));

        return readings;
    }

    private static IEnumerable<SensorReading> CreateSeries(
        Sensor sensor,
        string unit,
        DateTime now,
        double[] values,
        double[] hoursAgo)
    {
        return values.Select((value, index) =>
        {
            var timestamp = now.AddHours(-hoursAgo[index]);

            return new SensorReading
            {
                SensorId = sensor.Id,
                Timestamp = timestamp,
                Value = value,
                Unit = unit,
                RawData = $$"""{"source":"development-seed","deviceId":"{{sensor.DeviceId}}","value":{{value.ToString(System.Globalization.CultureInfo.InvariantCulture)}}}""",
                Metadata = new Dictionary<string, string>
                {
                    ["source"] = "development-seed",
                    ["deviceId"] = sensor.DeviceId
                }
            };
        });
    }

    private static List<SensorStatusUpdate> CreateStatusUpdates(IReadOnlyDictionary<string, Sensor> sensors, DateTime now)
    {
        return sensors.Values.Select(sensor => new SensorStatusUpdate
        {
            SensorId = sensor.Id,
            Timestamp = sensor.LastCommunication,
            Status = sensor.IsActive ? "Online" : "Offline",
            BatteryLevel = sensor.BatteryLevel,
            SignalStrength = sensor.IsActive ? "-62dBm" : "-91dBm",
            Metadata = new Dictionary<string, string>
            {
                ["firmwareVersion"] = sensor.FirmwareVersion ?? "unknown",
                ["source"] = "development-seed",
                ["updatedAt"] = now.ToString("O")
            }
        }).ToList();
    }

    private List<SensorAlert> CreateAlerts(IReadOnlyDictionary<string, Sensor> sensors, DateTime now)
    {
        var acknowledgedById = _context.Users.AsNoTracking()
            .Where(user => user.Enable)
            .OrderBy(user => user.Name)
            .Select(user => (Guid?)user.Id)
            .FirstOrDefault();

        return
        [
            CreateAlert(sensors["water"], AlertType.BatteryLow, "Bateria baixa no sensor de agua do jardim.", now.AddMinutes(-35), false, null, null),
            CreateAlert(sensors["temperature"], AlertType.ThresholdExceeded, "Temperatura da sala acima do limite maximo configurado.", now.AddMinutes(-22), false, null, null),
            CreateAlert(sensors["motion"], AlertType.DeviceOffline, "Sensor de movimento do escritorio sem comunicacao recente.", now.AddHours(-3), false, null, null),
            CreateAlert(sensors["gas"], AlertType.ThresholdExceeded, "Pico de gas detectado na cozinha e reconhecido pelo operador.", now.AddHours(-5), true, now.AddHours(-4.5), acknowledgedById),
            CreateAlert(sensors["humidity"], AlertType.ThresholdBelowMinimum, "Umidade abaixo do limite minimo durante a madrugada.", now.AddDays(-2), true, now.AddDays(-2).AddHours(1), acknowledgedById),
            CreateAlert(sensors["smoke"], AlertType.ThresholdExceeded, "Fumaca acima do limite na cozinha.", now.AddMinutes(-8), false, null, null)
        ];
    }

    private static SensorAlert CreateAlert(
        Sensor sensor,
        AlertType type,
        string message,
        DateTime timestamp,
        bool isAcknowledged,
        DateTime? acknowledgedAt,
        Guid? acknowledgedById)
    {
        return new SensorAlert
        {
            SensorId = sensor.Id,
            Type = type,
            Message = message,
            Timestamp = timestamp,
            Created = timestamp,
            Modified = acknowledgedAt ?? timestamp,
            IsAcknowledged = isAcknowledged,
            AcknowledgedAt = acknowledgedAt,
            AcknowledgedById = acknowledgedById
        };
    }
}
