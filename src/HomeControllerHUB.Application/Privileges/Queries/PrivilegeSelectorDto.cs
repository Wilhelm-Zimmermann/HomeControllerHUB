using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Mappings;

namespace HomeControllerHUB.Application.Privileges.Queries;

public class PrivilegeSelectorDto : IMapFrom<Privilege>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    [MapFrom(nameof(Privilege.Description))]
    public string Code { get; set; }
    public string? Domain { get; set; }
    public string? DomainDisplayName { get; set; }
    public string? Action { get; set; }
    public string? ActionDisplayName { get; set; }
    public string? Description { get; set; }

    public void Mapping(AutoMapper.Profile profile)
    {
        profile.CreateMap<Privilege, PrivilegeSelectorDto>()
            .ForMember(d => d.Code, opt => opt.MapFrom(s => s.Description))
            .ForMember(d => d.Domain, opt => opt.MapFrom(s => s.Domain.Name))
            .ForMember(d => d.DomainDisplayName, opt => opt.MapFrom(s => s.Domain.Description))
            .ForMember(d => d.Action, opt => opt.MapFrom(s => s.Actions))
            .ForMember(d => d.ActionDisplayName, opt => opt.MapFrom(s => s.Actions));
    }
}

