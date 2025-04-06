using HomeControllerHUB.Shared.Normalize;

namespace HomeControllerHUB.Domain.Entities;

public class Establishment : Base
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    [Normalized(nameof(Name))]
    public string? NormalizedName { get; set; }
    public string? SiteName { get; set; }
    [Normalized(nameof(SiteName))]
    public string? NormalizedSiteName { get; set; }
    public string Document { get; set; } = null!;
    public bool Enable { get; set; } = false;
    public bool IsMaster { get; set; } = false;
    
    // Subscription information
    public Guid? SubscriptionPlanId { get; set; }
    public virtual SubscriptionPlan? SubscriptionPlan { get; set; }
    public DateTime? SubscriptionEndDate { get; set; }
    
    public virtual IList<ApplicationUser> Users { get; private set; }
    public virtual IList<UserEstablishment> UserEstablishments { get; private set; }
    public virtual IList<Location> Locations { get; private set; }
    public virtual IList<Sensor> Sensors { get; private set; }
}