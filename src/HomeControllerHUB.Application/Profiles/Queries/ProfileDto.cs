using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Mappings;

namespace HomeControllerHUB.Application.Profiles.Queries;

public class ProfileDto : IMapFrom<Profile>
{
    public Guid Id { get; set; }
    public Guid EstablishmentId { get; set; }
    public string? Name { get; set; }
    public string? NormalizedName { get; set; }
    public string? Description { get; set; }
    public string? NormalizedDescription { get; set; }
    public bool Enable { get; set; }
    public List<Guid> PrivilegeIds { get; set; } = [];
    public List<ProfilePrivilegeDto> Privileges { get; set; } = [];

    public void Mapping(AutoMapper.Profile profile)
    {
        profile.CreateMap<Profile, ProfileDto>()
            .ForMember(d => d.PrivilegeIds, opt => opt.MapFrom(s =>
                s.ProfilePrivileges.Select(pp => pp.PrivilegeId).ToList()))
            .ForMember(d => d.Privileges, opt => opt.MapFrom(s =>
                s.ProfilePrivileges.Select(pp => new ProfilePrivilegeDto
                    {
                        PrivilegeId = pp.PrivilegeId,
                        Domain = pp.Privilege.Domain.Name,
                        DomainDisplayName = pp.Privilege.Domain.Description,
                        Action = pp.Privilege.Actions,
                        ActionDisplayName = pp.Privilege.Actions,
                        Description = pp.Privilege.Description
                    }).ToList()));
    }
}

public class ProfilePrivilegeDto
{
    public Guid PrivilegeId { get; set; }
    public string? Domain { get; set; }
    public string? DomainDisplayName { get; set; }
    public string? Action { get; set; }
    public string? ActionDisplayName { get; set; }
    public string? Description { get; set; }
}
