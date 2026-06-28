using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Interfaces;
namespace HomeControllerHUB.Application.SubscriptionPlans.Queries;

public class SubscriptionPlanDto : IPaginatedDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int MaxSensors { get; set; }
    public int DataRetentionDays { get; set; }
    public int AlertsPerMonth { get; set; }
    public bool IncludesReporting { get; set; }
    public bool IncludesApiAccess { get; set; }
    
    public DateTime Created { get; set; }
    public DateTime Modified { get; set; }
} 