using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

var console = new SimulatorConsole();
var configPath = ResolveConfigPath(args);

if (configPath is null)
{
    console.WriteError("Config file not found. Copy sensor-simulator.example.json to sensor-simulator.local.json or pass --config <path>.");
    return 1;
}

SimulatorConfig config;
try
{
    var configJson = await File.ReadAllTextAsync(configPath);
    config = JsonSerializer.Deserialize<SimulatorConfig>(configJson, JsonDefaults.Options) ?? new SimulatorConfig();
    config.Sensors ??= [];
}
catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
{
    console.WriteError($"Could not load config file: {ex.Message}");
    return 1;
}

var validationErrors = ConfigValidator.Validate(config).ToArray();
if (validationErrors.Length > 0)
{
    foreach (var error in validationErrors)
    {
        console.WriteError(error);
    }

    return 1;
}

using var cancellation = new CancellationTokenSource();
Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    console.WriteInfo("Stopping simulator...");
    cancellation.Cancel();
};

if (config.RunForSeconds is > 0)
{
    cancellation.CancelAfter(TimeSpan.FromSeconds(config.RunForSeconds.Value));
}

var endpoint = BuildEndpoint(config.ApiBaseUrl);
using var httpClient = new HttpClient
{
    Timeout = TimeSpan.FromSeconds(20)
};

var sensorStates = config.Sensors
    .Select(sensor => new SensorState(sensor))
    .ToArray();

console.WriteInfo($"Sensor simulator started with {sensorStates.Length} sensor(s). Config: {Path.GetFullPath(configPath)}");
console.WriteInfo($"Posting to {endpoint}. Press Ctrl+C to stop.");

try
{
    while (!cancellation.IsCancellationRequested)
    {
        foreach (var sensorState in sensorStates)
        {
            if (cancellation.IsCancellationRequested)
            {
                break;
            }

            var (payload, isDuplicate) = sensorState.CreatePayload();
            await PostReadingAsync(httpClient, endpoint, sensorState.Sensor, payload, isDuplicate, console, cancellation.Token);
        }

        await Task.Delay(TimeSpan.FromSeconds(config.IntervalSeconds), cancellation.Token);
    }
}
catch (OperationCanceledException)
{
    // Normal shutdown through Ctrl+C or runForSeconds.
}

console.WriteInfo("Sensor simulator stopped.");
return 0;

static string? ResolveConfigPath(string[] args)
{
    var explicitPath = TryGetConfigArgument(args);
    if (!string.IsNullOrWhiteSpace(explicitPath))
    {
        return File.Exists(explicitPath) ? explicitPath : null;
    }

    var candidates = new[]
    {
        Path.Combine(Environment.CurrentDirectory, "sensor-simulator.local.json"),
        Path.Combine(Environment.CurrentDirectory, "tools", "HomeControllerHUB.SensorSimulator", "sensor-simulator.local.json"),
        Path.Combine(AppContext.BaseDirectory, "sensor-simulator.local.json"),
        Path.Combine(AppContext.BaseDirectory, "sensor-simulator.example.json"),
        Path.Combine(Environment.CurrentDirectory, "tools", "HomeControllerHUB.SensorSimulator", "sensor-simulator.example.json"),
        Path.Combine(Environment.CurrentDirectory, "sensor-simulator.example.json")
    };

    return candidates.FirstOrDefault(File.Exists);
}

static string? TryGetConfigArgument(string[] args)
{
    for (var i = 0; i < args.Length; i++)
    {
        if (args[i] is "--config" or "-c")
        {
            return i + 1 < args.Length ? args[i + 1] : null;
        }
    }

    return null;
}

static Uri BuildEndpoint(string apiBaseUrl)
{
    var baseUrl = apiBaseUrl.EndsWith("/", StringComparison.Ordinal)
        ? apiBaseUrl
        : apiBaseUrl + "/";

    return new Uri(new Uri(baseUrl), "SensorReadings/ingest");
}

static async Task PostReadingAsync(
    HttpClient httpClient,
    Uri endpoint,
    SensorConfig sensor,
    SensorReadingPayload payload,
    bool isDuplicate,
    SimulatorConsole console,
    CancellationToken cancellationToken)
{
    using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
    {
        Content = JsonContent.Create(payload, options: JsonDefaults.Options)
    };

    request.Headers.TryAddWithoutValidation("X-Api-Key", sensor.ApiKey);

    try
    {
        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            var ingestResponse = await response.Content.ReadFromJsonAsync<IngestResponse>(JsonDefaults.Options, cancellationToken);
            var status = ingestResponse?.Status ?? "Processed";

            if (string.Equals(status, "Duplicate", StringComparison.OrdinalIgnoreCase) || isDuplicate)
            {
                console.WriteReading(sensor.DeviceId, "duplicate ignored");
                return;
            }

            if (string.Equals(status, "Queued", StringComparison.OrdinalIgnoreCase))
            {
                console.WriteReading(sensor.DeviceId, "queued");
                return;
            }

            console.WriteReading(
                sensor.DeviceId,
                $"{FormatValue(payload.Value)} {payload.Unit}".TrimEnd() + $" | status={status}");
            return;
        }

        console.WriteReading(sensor.DeviceId, $"failed {(int)response.StatusCode} {SafeReasonPhrase(response.StatusCode)}");
    }
    catch (HttpRequestException ex)
    {
        console.WriteReading(sensor.DeviceId, $"failed request error: {ex.Message}");
    }
    catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
    {
        console.WriteReading(sensor.DeviceId, "failed request timeout");
    }
}

static string SafeReasonPhrase(HttpStatusCode statusCode)
{
    return statusCode switch
    {
        HttpStatusCode.Unauthorized => "Unauthorized",
        HttpStatusCode.Forbidden => "Forbidden",
        HttpStatusCode.NotFound => "NotFound",
        HttpStatusCode.TooManyRequests => "TooManyRequests",
        HttpStatusCode.BadRequest => "BadRequest",
        _ => statusCode.ToString()
    };
}

static string FormatValue(double value)
{
    return value.ToString("0.##", CultureInfo.InvariantCulture);
}

internal sealed class SensorState
{
    private readonly Random _random = new();
    private readonly double _span;
    private double _batteryLevel;
    private double _currentValue;
    private int _sequence;

    public SensorState(SensorConfig sensor)
    {
        Sensor = sensor;
        _span = Math.Max(sensor.MaxValue - sensor.MinValue, 1);
        _batteryLevel = sensor.BatteryStart;
        _currentValue = NextNormalValue();
    }

    public SensorConfig Sensor { get; }

    public SensorReadingPayload? LastPayload { get; private set; }

    public (SensorReadingPayload Payload, bool IsDuplicate) CreatePayload()
    {
        if (LastPayload is not null && Roll(Sensor.DuplicateChancePercent))
        {
            return (LastPayload, true);
        }

        _sequence++;
        _batteryLevel = Math.Max(0, _batteryLevel - _random.NextDouble() * 0.05);

        var timestamp = DateTime.UtcNow;
        var payload = new SensorReadingPayload
        {
            MessageId = $"{Sensor.DeviceId}-{timestamp:yyyyMMddHHmmssfff}-{_random.Next(1000, 9999)}",
            DeviceId = Sensor.DeviceId,
            Timestamp = timestamp,
            Value = NextValue(),
            Unit = Sensor.Unit,
            BatteryLevel = Math.Round(_batteryLevel, 1),
            RawData = new Dictionary<string, object?>
            {
                ["firmware"] = "1.0.3",
                ["simulated"] = true,
                ["source"] = "HomeControllerHUB.SensorSimulator",
                ["sensorName"] = Sensor.Name,
                ["sequence"] = _sequence
            }
        };

        LastPayload = payload;
        return (payload, false);
    }

    private double NextValue()
    {
        if (IsBinarySensor())
        {
            return _random.NextDouble() < 0.85 ? 0 : 1;
        }

        if (Roll(Sensor.SpikeChancePercent))
        {
            var spike = _random.NextDouble() < 0.5
                ? Sensor.MinValue - _random.NextDouble() * _span * 0.2
                : Sensor.MaxValue + _random.NextDouble() * _span * 0.2;

            _currentValue = spike;
            return Round(spike);
        }

        var drift = (_random.NextDouble() - 0.5) * _span * 0.08;
        _currentValue = Math.Clamp(_currentValue + drift, Sensor.NormalMin, Sensor.NormalMax);
        return Round(_currentValue);
    }

    private double NextNormalValue()
    {
        return Sensor.NormalMin + _random.NextDouble() * (Sensor.NormalMax - Sensor.NormalMin);
    }

    private bool Roll(double chancePercent)
    {
        return chancePercent > 0 && _random.NextDouble() * 100 < chancePercent;
    }

    private bool IsBinarySensor()
    {
        return Sensor.MinValue == 0
               && Sensor.MaxValue == 1
               && Sensor.NormalMin == 0
               && Sensor.NormalMax == 1;
    }

    private static double Round(double value)
    {
        return Math.Round(value, Math.Abs(value) >= 100 ? 1 : 2);
    }
}

internal sealed class SimulatorConsole
{
    public void WriteInfo(string message)
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {message}");
    }

    public void WriteError(string message)
    {
        Console.Error.WriteLine($"[{DateTime.Now:HH:mm:ss}] {message}");
    }

    public void WriteReading(string deviceId, string message)
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {deviceId} -> {message}");
    }
}

internal static class ConfigValidator
{
    public static IEnumerable<string> Validate(SimulatorConfig config)
    {
        if (!Uri.TryCreate(config.ApiBaseUrl, UriKind.Absolute, out var apiBaseUrl)
            || apiBaseUrl.Scheme is not ("http" or "https"))
        {
            yield return "apiBaseUrl must be an absolute http or https URL.";
        }

        if (config.IntervalSeconds <= 0)
        {
            yield return "intervalSeconds must be greater than zero.";
        }

        if (config.RunForSeconds is <= 0)
        {
            yield return "runForSeconds must be greater than zero when configured.";
        }

        if (config.Sensors.Count == 0)
        {
            yield return "At least one sensor must be configured.";
        }

        for (var i = 0; i < config.Sensors.Count; i++)
        {
            var sensor = config.Sensors[i];
            var prefix = $"sensors[{i}]";

            if (string.IsNullOrWhiteSpace(sensor.DeviceId))
            {
                yield return $"{prefix}.deviceId is required.";
            }

            if (string.IsNullOrWhiteSpace(sensor.ApiKey))
            {
                yield return $"{prefix}.apiKey is required.";
            }

            if (sensor.MinValue > sensor.MaxValue)
            {
                yield return $"{prefix}.minValue must be less than or equal to maxValue.";
            }

            if (sensor.NormalMin > sensor.NormalMax)
            {
                yield return $"{prefix}.normalMin must be less than or equal to normalMax.";
            }

            if (sensor.NormalMin < sensor.MinValue || sensor.NormalMax > sensor.MaxValue)
            {
                yield return $"{prefix}.normalMin/normalMax must stay within minValue/maxValue.";
            }

            if (sensor.SpikeChancePercent is < 0 or > 100)
            {
                yield return $"{prefix}.spikeChancePercent must be between 0 and 100.";
            }

            if (sensor.DuplicateChancePercent is < 0 or > 100)
            {
                yield return $"{prefix}.duplicateChancePercent must be between 0 and 100.";
            }

            if (sensor.BatteryStart is < 0 or > 100)
            {
                yield return $"{prefix}.batteryStart must be between 0 and 100.";
            }
        }
    }
}

internal sealed class SimulatorConfig
{
    public string ApiBaseUrl { get; set; } = "http://localhost:6001/api/v1";

    public int IntervalSeconds { get; set; } = 5;

    public int? RunForSeconds { get; set; }

    public List<SensorConfig> Sensors { get; set; } = [];
}

internal sealed class SensorConfig
{
    public string Name { get; set; } = "";

    public string DeviceId { get; set; } = "";

    public string ApiKey { get; set; } = "";

    public string? Unit { get; set; }

    public double MinValue { get; set; }

    public double MaxValue { get; set; }

    public double NormalMin { get; set; }

    public double NormalMax { get; set; }

    public double SpikeChancePercent { get; set; }

    public double DuplicateChancePercent { get; set; }

    public double BatteryStart { get; set; } = 100;
}

internal sealed class SensorReadingPayload
{
    public string MessageId { get; set; } = "";

    public string DeviceId { get; set; } = "";

    public DateTime Timestamp { get; set; }

    public double Value { get; set; }

    public string? Unit { get; set; }

    public double BatteryLevel { get; set; }

    public Dictionary<string, object?> RawData { get; set; } = [];
}

internal sealed class IngestResponse
{
    public string? Status { get; set; }
}

internal static class JsonDefaults
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };
}
