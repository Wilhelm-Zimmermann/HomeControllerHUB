using HomeControllerHUB.Infra.DatabaseContext;
using HomeControllerHUB.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HomeControllerHUB.Infra.Services;

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
        var deletedReadings = await dbContext.SensorReadings
            .Where(r => sensorIds.Contains(r.SensorId) && r.Timestamp < cutoffDate)
            .ExecuteDeleteAsync(stoppingToken);
            
        _logger.LogInformation(
            "Deleted {DeletedReadings} sensor readings older than {CutoffDate} for establishment {EstablishmentId}", 
            deletedReadings,
            cutoffDate, 
            establishmentId);
            
        // Also clean up old acknowledged alerts
        var threeMonthsAgo = now.AddMonths(-3);
        var deletedAlerts = await dbContext.SensorAlerts
            .Where(a => sensorIds.Contains(a.SensorId) && a.IsAcknowledged && a.Timestamp < threeMonthsAgo)
            .ExecuteDeleteAsync(stoppingToken);
            
        _logger.LogInformation(
            "Deleted {DeletedAlerts} acknowledged sensor alerts older than {ThreeMonthsAgo} for establishment {EstablishmentId}", 
            deletedAlerts,
            threeMonthsAgo, 
            establishmentId);
    }
} 