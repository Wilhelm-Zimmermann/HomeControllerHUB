using System.Linq.Expressions;
using HomeControllerHUB.Domain.Entities;

namespace HomeControllerHUB.Application.Profiles.Queries;

public partial class GetProfilePaginatedDto
{
    public static readonly Expression<Func<Profile, GetProfilePaginatedDto>> Projection = profile => new GetProfilePaginatedDto
    {
        Id = profile.Id,
        EstablishmentId = profile.EstablishmentId,
        Name = profile.Name,
        NormalizedName = profile.NormalizedName,
        Description = profile.Description,
        NormalizedDescription = profile.NormalizedDescription,
        Enable = profile.Enable,
        UsersCount = profile.UserProfiles.Count,
        PrivilegesCount = profile.ProfilePrivileges.Count,
        Created = profile.Created,
        Modified = profile.Modified
    };
}
