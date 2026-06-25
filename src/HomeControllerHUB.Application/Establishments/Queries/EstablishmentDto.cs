using AutoMapper;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Mappings;
using AutoMapperProfile = AutoMapper.Profile;

namespace HomeControllerHUB.Application.Establishments.Queries;

public class EstablishmentDto : IMapFrom<Establishment>
{
    public Guid Id { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? SiteName { get; set; }
    public string? Document { get; set; }
    public bool Enable { get; set; } = false;
    public bool IsMaster { get; set; } = false;
    public Guid? SubscriptionPlanId { get; set; }
    public string? SubscriptionPlanName { get; set; }
    public DateTime? SubscriptionEndDate { get; set; }
    public DateTime Created { get; set; }
    public DateTime Modified { get; set; }
    public List<Guid> UserIds { get; set; } = new();
    public List<EstablishmentUserDto> Users { get; set; } = new();

    public void Mapping(AutoMapperProfile profile)
    {
        profile.CreateMap<Establishment, EstablishmentDto>()
            .ForMember(
                d => d.UserIds,
                opt => opt.MapFrom(s =>
                    s.UserEstablishments.Select(userEstablishment => userEstablishment.UserId)))
            .ForMember(
                d => d.Users,
                opt => opt.MapFrom(s => s.UserEstablishments));
    }
}

public class EstablishmentUserDto : IMapFrom<UserEstablishment>
{
    public Guid UserId { get; set; }
    public string? Name { get; set; }
    public string? Login { get; set; }
    public string? Email { get; set; }

    public void Mapping(AutoMapperProfile profile)
    {
        profile.CreateMap<UserEstablishment, EstablishmentUserDto>()
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.User.Name))
            .ForMember(d => d.Login, opt => opt.MapFrom(s => s.User.Login))
            .ForMember(d => d.Email, opt => opt.MapFrom(s => s.User.Email));
    }
}
