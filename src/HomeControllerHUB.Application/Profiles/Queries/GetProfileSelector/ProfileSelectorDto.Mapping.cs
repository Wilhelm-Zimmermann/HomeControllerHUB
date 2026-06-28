using System.Linq.Expressions;
using HomeControllerHUB.Domain.Entities;

namespace HomeControllerHUB.Application.Profiles.Queries.GetProfileSelector;

public partial class ProfileSelectorDto
{
    public static readonly Expression<Func<Profile, ProfileSelectorDto>> Projection = profile => new ProfileSelectorDto
    {
        Id = profile.Id,
        Name = profile.Name ?? string.Empty
    };
}
