using HomeControllerHUB.Shared.Normalize;

namespace HomeControllerHUB.Domain.Entities;

public class Location : Base
{
    public Guid EstablishmentId { get; set; }
    public virtual Establishment Establishment { get; set; } = null!;
    
    public string Name { get; set; } = null!;
    [Normalized(nameof(Name))]
    public string? NormalizedName { get; set; }
    
    public string? Description { get; set; }
    [Normalized(nameof(Description))]
    public string? NormalizedDescription { get; set; }
    
    public LocationType Type { get; set; }
    
    public Guid? ParentLocationId { get; set; }
    public virtual Location? ParentLocation { get; set; }
    
    public virtual IList<Location> ChildLocations { get; private set; }
    public virtual IList<Sensor> Sensors { get; private set; }
}

public enum LocationType
{
    Building,
    Floor,
    Room,
    Area,
    Equipment
} 