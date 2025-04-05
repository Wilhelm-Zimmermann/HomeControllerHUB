using HomeControllerHUB.Shared.Normalize;

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
    
    public virtual IList<Establishment> Establishments { get; private set; }
} 