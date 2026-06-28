using HomeControllerHUB.Domain.Entities;
namespace HomeControllerHUB.Application.Locations.Queries;

public partial class LocationHierarchyDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public LocationType Type { get; set; }
    public string TypeName => Type.ToString();
    public Guid? ParentLocationId { get; set; }
    public List<LocationHierarchyDto> Children { get; set; } = new List<LocationHierarchyDto>();
} 
