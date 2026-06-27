using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Domain.Mappings;

namespace HomeControllerHUB.Application.Profiles.Queries;

public class GetProfilePaginatedDto : IMapFrom<Profile>, IPaginatedDto
{
    public Guid Id { get; set; }
    public Guid EstablishmentId { get; set; }
    public string? Name { get; set; }
    public string? NormalizedName { get; set; }
    public string? Description { get; set; }
    public string? NormalizedDescription { get; set; }
    public bool Enable { get; set; }
    public int UsersCount { get; set; }
    public int PrivilegesCount { get; set; }
    public DateTime Created { get; set; }
    public DateTime Modified { get; set; }

    public void Mapping(AutoMapper.Profile profile)
    {
        profile.CreateMap<Profile, GetProfilePaginatedDto>()
            .ForMember(d => d.UsersCount, opt => opt.MapFrom(s => s.UserProfiles.Count))
            .ForMember(d => d.PrivilegesCount, opt => opt.MapFrom(s => s.ProfilePrivileges.Count));
    }
}
