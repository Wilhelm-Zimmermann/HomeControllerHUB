namespace HomeControllerHUB.Application.Sensors.Commands.MonitorSensorHealth;

public class SensorHealthMonitoringOptions
{
    public const string SectionName = "SensorHealthMonitoring";
    public const int DefaultIntervalSeconds = 60;
    public const int DefaultOfflineThresholdMinutes = 10;
    public const double DefaultLowBatteryThreshold = 20;
    public const int MinimumIntervalSeconds = 1;
    public const int MinimumOfflineThresholdMinutes = 1;

    public bool Enabled { get; set; } = true;
    public int IntervalSeconds { get; set; } = DefaultIntervalSeconds;
    public int OfflineThresholdMinutes { get; set; } = DefaultOfflineThresholdMinutes;
    public double LowBatteryThreshold { get; set; } = DefaultLowBatteryThreshold;

    public int EffectiveIntervalSeconds => Math.Max(MinimumIntervalSeconds, IntervalSeconds);
    public int EffectiveOfflineThresholdMinutes => Math.Max(MinimumOfflineThresholdMinutes, OfflineThresholdMinutes);
}
