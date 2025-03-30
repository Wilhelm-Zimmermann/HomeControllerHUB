using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Mappings;

namespace HomeControllerHUB.Application.Profiles.Queries.GetProfileSelector;

public class ProfileSelectorDto : IMapFrom<Profile>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string? Code { get; set; }
}