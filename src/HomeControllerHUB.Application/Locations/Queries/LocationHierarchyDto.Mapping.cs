using System.Linq.Expressions;
using HomeControllerHUB.Domain.Entities;

namespace HomeControllerHUB.Application.Locations.Queries;

public partial class LocationHierarchyDto
{
    public static readonly Expression<Func<Location, LocationHierarchyDto>> Projection = location => new LocationHierarchyDto
    {
        Id = location.Id,
        Name = location.Name,
        Description = location.Description,
        Type = location.Type,
        ParentLocationId = location.ParentLocationId
    };

    public static LocationHierarchyDto FromEntity(Location location)
    {
        return new LocationHierarchyDto
        {
            Id = location.Id,
            Name = location.Name,
            Description = location.Description,
            Type = location.Type,
            ParentLocationId = location.ParentLocationId
        };
    }
}
