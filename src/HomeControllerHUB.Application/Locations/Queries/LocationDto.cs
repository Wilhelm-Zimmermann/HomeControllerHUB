using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Domain.Mappings;

namespace HomeControllerHUB.Application.Locations.Queries;

public class LocationDto : IMapFrom<Location>, IPaginatedDto
{
    public Guid Id { get; set; }
    public Guid EstablishmentId { get; set; }
    public string EstablishmentName { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public LocationType Type { get; set; }
    public string TypeName => Type.ToString();
    public Guid? ParentLocationId { get; set; }
    public string? ParentLocationName { get; set; }
    public DateTime Created { get; set; }
    public DateTime Modified { get; set; }
} 