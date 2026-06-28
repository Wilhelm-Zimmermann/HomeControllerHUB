using System.Linq.Expressions;
using HomeControllerHUB.Domain.Entities;

namespace HomeControllerHUB.Application.Locations.Queries;

public partial class LocationDto
{
    public static readonly Expression<Func<Location, LocationDto>> Projection = location => new LocationDto
    {
        Id = location.Id,
        EstablishmentId = location.EstablishmentId,
        EstablishmentName = location.Establishment != null ? location.Establishment.Name : string.Empty,
        Name = location.Name,
        Description = location.Description,
        Type = location.Type,
        ParentLocationId = location.ParentLocationId,
        ParentLocationName = location.ParentLocation != null ? location.ParentLocation.Name : null,
        Created = location.Created,
        Modified = location.Modified
    };

    public static LocationDto FromEntity(Location location)
    {
        return new LocationDto
        {
            Id = location.Id,
            EstablishmentId = location.EstablishmentId,
            EstablishmentName = location.Establishment?.Name ?? string.Empty,
            Name = location.Name,
            Description = location.Description,
            Type = location.Type,
            ParentLocationId = location.ParentLocationId,
            ParentLocationName = location.ParentLocation?.Name,
            Created = location.Created,
            Modified = location.Modified
        };
    }
}
