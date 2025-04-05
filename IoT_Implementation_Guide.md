# HomeControllerHUB IoT Implementation Guide

## Overview
This guide provides implementation details for extending the HomeControllerHUB system to support IoT functionality, specifically focused on sensor data collection, monitoring, and management for establishments. The system will allow establishments to register, monitor, and analyze data from various sensors placed in different locations within their premises.

## Business Requirements

1. **Subscription-Based Service**:
   - Establishments will subscribe to the service monthly
   - Different subscription tiers may offer varying levels of service (data retention, number of sensors, etc.)

2. **Sensor Management**:
   - Register and manage multiple sensors per establishment
   - Organize sensors by location within the establishment
   - Monitor sensor status (online/offline, battery level, etc.)

3. **Data Collection**:
   - Collect and store time-series data from sensors
   - Support various sensor types (temperature, humidity, motion, etc.)
   - Provide mechanism for ESP32/Arduino devices to send data to the backend

4. **Data Visualization and Alerts**:
   - Display sensor data in dashboards
   - Set up alerts for threshold violations
   - Generate reports on sensor data

## Entity Design

Following the established DDD architecture, we'll define the following entities:

### 1. Sensor
```csharp
namespace HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Shared.Normalize;

public class Sensor : Base
{
    public Guid EstablishmentId { get; set; }
    public virtual Establishment Establishment { get; set; } = null!;
    
    public Guid LocationId { get; set; }
    public virtual Location Location { get; set; } = null!;
    
    public string Name { get; set; } = null!;
    [Normalized(nameof(Name))]
    public string? NormalizedName { get; set; }
    
    public string DeviceId { get; set; } = null!; // Unique identifier for the physical device
    public SensorType Type { get; set; }
    public string Model { get; set; } = null!;
    public string? FirmwareVersion { get; set; }
    public string? ApiKey { get; set; } // API key for sensor authentication
    
    public double? MinThreshold { get; set; } // Minimum value for alerts
    public double? MaxThreshold { get; set; } // Maximum value for alerts
    
    public bool IsActive { get; set; } = true;
    public DateTime LastCommunication { get; set; }
    public double? BatteryLevel { get; set; }
    
    public virtual IList<SensorReading> Readings { get; private set; } = new List<SensorReading>();
    public virtual IList<SensorAlert> Alerts { get; private set; } = new List<SensorAlert>();
}

public enum SensorType
{
    Temperature,
    Humidity,
    Pressure,
    Light,
    Motion,
    Door,
    Water,
    Smoke,
    Gas,
    Electricity,
    Custom
}
```

### 2. Location
```csharp
namespace HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Shared.Normalize;

public class Location : Base
{
    public Guid EstablishmentId { get; set; }
    public virtual Establishment Establishment { get; set; } = null!;
    
    public string Name { get; set; } = null!; // Kitchen, Freezer, etc.
    [Normalized(nameof(Name))]
    public string? NormalizedName { get; set; }
    
    public string? Description { get; set; }
    [Normalized(nameof(Description))]
    public string? NormalizedDescription { get; set; }
    
    public LocationType Type { get; set; }
    
    public Guid? ParentLocationId { get; set; } // For hierarchical locations
    public virtual Location? ParentLocation { get; set; }
    
    public virtual IList<Location> ChildLocations { get; private set; } = new List<Location>();
    public virtual IList<Sensor> Sensors { get; private set; } = new List<Sensor>();
}

public enum LocationType
{
    Building,
    Floor,
    Room,
    Area,
    Equipment
}
```

### 3. SensorReading
```csharp
namespace HomeControllerHUB.Domain.Entities;

public class SensorReading : Base
{
    public Guid SensorId { get; set; }
    public virtual Sensor Sensor { get; set; } = null!;
    
    public DateTime Timestamp { get; set; }
    public double Value { get; set; }
    public string? Unit { get; set; } // °C, %, ppm, etc.
    
    public string? RawData { get; set; } // JSON or other format for additional data
}
```

### 4. SensorAlert
```csharp
namespace HomeControllerHUB.Domain.Entities;

public class SensorAlert : Base
{
    public Guid SensorId { get; set; }
    public virtual Sensor Sensor { get; set; } = null!;
    
    public AlertType Type { get; set; }
    public string Message { get; set; } = null!;
    public DateTime Timestamp { get; set; }
    public bool IsAcknowledged { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public Guid? AcknowledgedById { get; set; }
    public virtual ApplicationUser? AcknowledgedBy { get; set; }
}

public enum AlertType
{
    ThresholdExceeded,
    ThresholdBelowMinimum,
    DeviceOffline,
    BatteryLow,
    Error
}
```

### 5. SubscriptionPlan (Addition to Establishment)
```csharp
// Add to Establishment.cs
public Guid? SubscriptionPlanId { get; set; }
public virtual SubscriptionPlan? SubscriptionPlan { get; set; }
public DateTime? SubscriptionEndDate { get; set; }

// New entity
namespace HomeControllerHUB.Domain.Entities;

public class SubscriptionPlan : Base
{
    public string Name { get; set; } = null!;
    [Normalized(nameof(Name))]
    public string? NormalizedName { get; set; }
    
    public string? Description { get; set; }
    [Normalized(nameof(Description))]
    public string? NormalizedDescription { get; set; }
    
    public decimal Price { get; set; }
    public int MaxSensors { get; set; }
    public int DataRetentionDays { get; set; }
    public int AlertsPerMonth { get; set; }
    public bool IncludesReporting { get; set; }
    public bool IncludesApiAccess { get; set; }
    
    public virtual IList<Establishment> Establishments { get; private set; } = new List<Establishment>();
}
```

## Entity Configurations

Create configuration files for each entity in the `HomeControllerHUB.Domain/Entities/Configuration` folder. Here's an example for the Sensor entity:

```csharp
namespace HomeControllerHUB.Domain.Entities.Configuration;

public class SensorConfiguration : IEntityTypeConfiguration<Sensor>
{
    public void Configure(EntityTypeBuilder<Sensor> builder)
    {
        builder.ToTable("Sensors");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Name).HasMaxLength(255).IsRequired();
        builder.Property(x => x.DeviceId).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Model).HasMaxLength(100).IsRequired();
        builder.Property(x => x.FirmwareVersion).HasMaxLength(50);
        builder.Property(x => x.ApiKey).HasMaxLength(100);
        
        builder.HasIndex(x => x.DeviceId).IsUnique();
        builder.HasIndex(x => x.NormalizedName);
        
        builder.HasOne(x => x.Establishment)
            .WithMany()
            .HasForeignKey(x => x.EstablishmentId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(x => x.Location)
            .WithMany(x => x.Sensors)
            .HasForeignKey(x => x.LocationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
```

Do the same for the other entities, ensuring appropriate relationships and constraints.

## Database Context Update

Update the `ApplicationDbContext` to include the new entities:

```csharp
public DbSet<Sensor> Sensors => Set<Sensor>();
public DbSet<Location> Locations => Set<Location>();
public DbSet<SensorReading> SensorReadings => Set<SensorReading>();
public DbSet<SensorAlert> SensorAlerts => Set<SensorAlert>();
public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();

// In OnModelCreating
modelBuilder.ApplyConfiguration(new SensorConfiguration());
modelBuilder.ApplyConfiguration(new LocationConfiguration());
modelBuilder.ApplyConfiguration(new SensorReadingConfiguration());
modelBuilder.ApplyConfiguration(new SensorAlertConfiguration());
modelBuilder.ApplyConfiguration(new SubscriptionPlanConfiguration());
```

## API Endpoints Implementation

Following the CQRS pattern, implement the necessary commands, queries, and controllers for each entity. Here are the key endpoints:

### Locations Controller

```csharp
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
public class LocationsController : ApiControllerBase
{
    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateLocationCommand command)
    {
        return await Mediator.Send(command);
    }
    
    [HttpPut]
    public async Task<ActionResult> Update(UpdateLocationCommand command)
    {
        await Mediator.Send(command);
        return NoContent();
    }
    
    [HttpDelete]
    public async Task<ActionResult> Delete(DeleteLocationCommand command)
    {
        await Mediator.Send(command);
        return NoContent();
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<LocationDto>> Get(Guid id)
    {
        return await Mediator.Send(new GetLocationQuery { Id = id });
    }
    
    [HttpGet]
    public async Task<ActionResult<PaginatedList<LocationDto>>> Get([FromQuery] GetLocationsQuery query)
    {
        return await Mediator.Send(query);
    }
    
    [HttpGet("list")]
    public async Task<ActionResult<List<LocationDto>>> GetList()
    {
        return await Mediator.Send(new GetLocationListQuery());
    }
    
    [HttpGet("hierarchical")]
    public async Task<ActionResult<List<LocationHierarchyDto>>> GetHierarchy([FromQuery] GetLocationHierarchyQuery query)
    {
        return await Mediator.Send(query);
    }
}
```

### Sensors Controller

```csharp
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
public class SensorsController : ApiControllerBase
{
    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateSensorCommand command)
    {
        return await Mediator.Send(command);
    }
    
    [HttpPut]
    public async Task<ActionResult> Update(UpdateSensorCommand command)
    {
        await Mediator.Send(command);
        return NoContent();
    }
    
    [HttpDelete]
    public async Task<ActionResult> Delete(DeleteSensorCommand command)
    {
        await Mediator.Send(command);
        return NoContent();
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<SensorDto>> Get(Guid id)
    {
        return await Mediator.Send(new GetSensorQuery { Id = id });
    }
    
    [HttpGet]
    public async Task<ActionResult<PaginatedList<SensorDto>>> Get([FromQuery] GetSensorsQuery query)
    {
        return await Mediator.Send(query);
    }
    
    [HttpGet("list")]
    public async Task<ActionResult<List<SensorDto>>> GetList()
    {
        return await Mediator.Send(new GetSensorListQuery());
    }
    
    [HttpGet("{id}/readings")]
    public async Task<ActionResult<PaginatedList<SensorReadingDto>>> GetReadings(Guid id, [FromQuery] GetSensorReadingsQuery query)
    {
        query.SensorId = id;
        return await Mediator.Send(query);
    }
    
    [HttpGet("{id}/alerts")]
    public async Task<ActionResult<PaginatedList<SensorAlertDto>>> GetAlerts(Guid id, [FromQuery] GetSensorAlertsQuery query)
    {
        query.SensorId = id;
        return await Mediator.Send(query);
    }
}
```

### Sensor Readings Data Collection API

Create a specialized controller for IoT devices to submit sensor readings:

```csharp
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
public class SensorDataController : ApiControllerBase
{
    [HttpPost("readings")]
    [AllowAnonymous] // This endpoint needs to be accessible by IoT devices
    public async Task<ActionResult> SubmitReading(SubmitSensorReadingCommand command)
    {
        // This will validate the API key in the handler
        await Mediator.Send(command);
        return Ok();
    }
    
    [HttpPost("readings/batch")]
    [AllowAnonymous] // This endpoint needs to be accessible by IoT devices
    public async Task<ActionResult> SubmitReadingBatch(SubmitSensorReadingBatchCommand command)
    {
        // This will validate the API key in the handler
        await Mediator.Send(command);
        return Ok();
    }
    
    [HttpPost("status")]
    [AllowAnonymous] // This endpoint needs to be accessible by IoT devices
    public async Task<ActionResult> UpdateStatus(UpdateSensorStatusCommand command)
    {
        // This will validate the API key in the handler
        await Mediator.Send(command);
        return Ok();
    }
}
```

## Command and Query Implementation

Implement the necessary commands and queries for each entity. Here are examples for sensor data collection:

### SubmitSensorReadingCommand

```csharp
namespace HomeControllerHUB.Application.Sensors.Commands.SubmitSensorReading;

public class SubmitSensorReadingCommand : IRequest
{
    public string DeviceId { get; set; } = null!;
    public string ApiKey { get; set; } = null!;
    public double Value { get; set; }
    public string? Unit { get; set; }
    public DateTime? Timestamp { get; set; } // If null, use server time
    public string? RawData { get; set; }
}

public class SubmitSensorReadingCommandValidator : AbstractValidator<SubmitSensorReadingCommand>
{
    public SubmitSensorReadingCommandValidator()
    {
        RuleFor(x => x.DeviceId).NotEmpty();
        RuleFor(x => x.ApiKey).NotEmpty();
        RuleFor(x => x.Value).NotNull();
    }
}

public class SubmitSensorReadingCommandHandler : IRequestHandler<SubmitSensorReadingCommand>
{
    private readonly ApplicationDbContext _context;
    private readonly IDateTime _dateTime;
    private readonly ISharedResource _sharedResource;

    public SubmitSensorReadingCommandHandler(
        ApplicationDbContext context,
        IDateTime dateTime,
        ISharedResource sharedResource)
    {
        _context = context;
        _dateTime = dateTime;
        _sharedResource = sharedResource;
    }

    public async Task<Unit> Handle(SubmitSensorReadingCommand request, CancellationToken cancellationToken)
    {
        // Find the sensor by DeviceId and validate ApiKey
        var sensor = await _context.Sensors
            .FirstOrDefaultAsync(s => s.DeviceId == request.DeviceId, cancellationToken);

        if (sensor == null || sensor.ApiKey != request.ApiKey)
        {
            throw new AppError(
                StatusCodes.Status401Unauthorized,
                _sharedResource["InvalidSensorAuthentication"],
                _sharedResource["InvalidDeviceIdOrApiKey"]);
        }

        if (!sensor.IsActive)
        {
            throw new AppError(
                StatusCodes.Status403Forbidden,
                _sharedResource["InactiveSensor"],
                _sharedResource["SensorIsNotActive"]);
        }

        // Create the sensor reading
        var reading = new SensorReading
        {
            SensorId = sensor.Id,
            Value = request.Value,
            Unit = request.Unit,
            Timestamp = request.Timestamp ?? _dateTime.Now,
            RawData = request.RawData
        };

        // Update sensor last communication time
        sensor.LastCommunication = _dateTime.Now;

        // Check if reading is outside thresholds and create alert if needed
        if ((sensor.MinThreshold.HasValue && request.Value < sensor.MinThreshold.Value) ||
            (sensor.MaxThreshold.HasValue && request.Value > sensor.MaxThreshold.Value))
        {
            var alertType = request.Value < sensor.MinThreshold 
                ? AlertType.ThresholdBelowMinimum 
                : AlertType.ThresholdExceeded;
                
            var alert = new SensorAlert
            {
                SensorId = sensor.Id,
                Type = alertType,
                Message = string.Format(
                    _sharedResource["SensorThresholdAlert"],
                    sensor.Name,
                    request.Value,
                    request.Unit),
                Timestamp = _dateTime.Now,
                IsAcknowledged = false
            };
            
            _context.SensorAlerts.Add(alert);
        }

        _context.SensorReadings.Add(reading);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
```

## IoT Device Communication

### ESP32/Arduino Integration

Create a simple library or code example for ESP32/Arduino devices to send data to the backend:

```cpp
#include <WiFi.h>
#include <HTTPClient.h>
#include <ArduinoJson.h>

// WiFi credentials
const char* ssid = "YOUR_WIFI_SSID";
const char* password = "YOUR_WIFI_PASSWORD";

// Sensor information
const char* server_url = "https://your-api-domain.com/api/v1/SensorData/readings";
const char* device_id = "DEVICE_ID_HERE";
const char* api_key = "API_KEY_HERE";

// Function to send sensor data
void sendSensorData(float value, const char* unit) {
  // Check WiFi connection
  if (WiFi.status() != WL_CONNECTED) {
    Serial.println("WiFi not connected. Attempting to reconnect...");
    WiFi.begin(ssid, password);
    delay(5000);
    if (WiFi.status() != WL_CONNECTED) {
      Serial.println("Failed to connect to WiFi. Will try again next time.");
      return;
    }
  }
  
  // Create HTTP client
  HTTPClient http;
  http.begin(server_url);
  http.addHeader("Content-Type", "application/json");
  
  // Create JSON payload
  StaticJsonDocument<200> doc;
  doc["deviceId"] = device_id;
  doc["apiKey"] = api_key;
  doc["value"] = value;
  doc["unit"] = unit;
  
  // Serialize JSON
  String requestBody;
  serializeJson(doc, requestBody);
  
  // Send POST request
  int httpResponseCode = http.POST(requestBody);
  
  // Check result
  if (httpResponseCode > 0) {
    String response = http.getString();
    Serial.println("HTTP Response code: " + String(httpResponseCode));
    Serial.println(response);
  } else {
    Serial.println("Error on sending POST: " + String(httpResponseCode));
  }
  
  http.end();
}

void setup() {
  Serial.begin(115200);
  
  // Connect to WiFi
  WiFi.begin(ssid, password);
  Serial.println("Connecting to WiFi");
  
  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }
  
  Serial.println("");
  Serial.println("WiFi connected");
  Serial.println("IP address: " + WiFi.localIP().toString());
  
  // Initialize sensors here
}

void loop() {
  // Read sensor value (example: DHT temperature sensor)
  float temperature = readTemperature(); // Implement this function for your specific sensor
  
  // Send data to server
  sendSensorData(temperature, "°C");
  
  // Wait before next reading
  delay(60000); // 1 minute
}
```

## Dashboard Implementation

Create a dashboard page that displays:

1. Overview of all sensors status
2. Recent alerts
3. Graphs for sensor readings over time
4. Ability to filter by location and sensor type

Implementation details will depend on your front-end technology, but ensure you create the necessary DTOs and queries to support these features.

## Data Retention Policy

Implement a background service to handle data retention based on subscription plans:

```csharp
namespace HomeControllerHUB.Application.Services;

public class DataRetentionService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<DataRetentionService> _logger;

    public DataRetentionService(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<DataRetentionService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Data Retention Service running at: {time}", DateTimeOffset.Now);
            
            try
            {
                await ProcessDataRetention(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during data retention processing");
            }

            // Run once a day
            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
        }
    }

    private async Task ProcessDataRetention(CancellationToken stoppingToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var dateTime = scope.ServiceProvider.GetRequiredService<IDateTime>();

        // Get establishments with their subscription plans
        var establishments = await dbContext.Establishments
            .Include(e => e.SubscriptionPlan)
            .ToListAsync(stoppingToken);

        foreach (var establishment in establishments)
        {
            if (establishment.SubscriptionPlan == null)
            {
                // Use default retention if no subscription plan
                await DeleteOldReadings(dbContext, establishment.Id, 30, dateTime.Now, stoppingToken);
            }
            else
            {
                await DeleteOldReadings(
                    dbContext, 
                    establishment.Id, 
                    establishment.SubscriptionPlan.DataRetentionDays, 
                    dateTime.Now, 
                    stoppingToken);
            }
        }
    }

    private async Task DeleteOldReadings(
        ApplicationDbContext dbContext, 
        Guid establishmentId, 
        int retentionDays, 
        DateTime now,
        CancellationToken stoppingToken)
    {
        var cutoffDate = now.AddDays(-retentionDays);
        
        // Get sensor IDs for this establishment
        var sensorIds = await dbContext.Sensors
            .Where(s => s.EstablishmentId == establishmentId)
            .Select(s => s.Id)
            .ToListAsync(stoppingToken);
            
        if (!sensorIds.Any())
        {
            return;
        }
            
        // Delete readings older than retention period
        await dbContext.SensorReadings
            .Where(r => sensorIds.Contains(r.SensorId) && r.Timestamp < cutoffDate)
            .ExecuteDeleteAsync(stoppingToken);
            
        _logger.LogInformation(
            "Deleted sensor readings older than {CutoffDate} for establishment {EstablishmentId}", 
            cutoffDate, 
            establishmentId);
    }
}
```

Register this service in your application startup:

```csharp
// In ConfigureServices.cs
public static IServiceCollection AddInfraServices(this IServiceCollection services)
{
    // Other registrations...
    
    services.AddHostedService<DataRetentionService>();
    
    return services;
}
```

## Migration Instructions

After implementing all the entities and configurations, create and apply a database migration:

```bash
dotnet ef migrations add AddIoTEntities -p src/HomeControllerHUB.Api
dotnet ef database update -p src/HomeControllerHUB.Api
```

## Testing

Create unit tests for your commands and queries, focusing on:

1. Sensor data submission validation
2. Threshold alert generation
3. Data retention logic
4. Location hierarchy retrieval

## Conclusion

This implementation guide provides a comprehensive framework for adding IoT sensor management capabilities to the HomeControllerHUB system. Following the established DDD and CQRS patterns, it extends the system with:

1. Entities for sensors, locations, readings, and alerts
2. API endpoints for sensor data collection and management
3. Integration examples for IoT devices
4. Data retention mechanisms based on subscription plans

With these implementations, establishments will be able to monitor various sensors throughout their premises, collect and analyze data, and receive alerts when measurements exceed defined thresholds.

Follow the existing project patterns for implementing commands, queries, and validators to ensure consistency with the system architecture. 