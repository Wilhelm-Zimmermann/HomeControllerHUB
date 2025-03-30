
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Mappings;

namespace HomeControllerHUB.Application.Generics.Queries;

public class GenericDto : IMapFrom<Generic>
{
    public Guid Id { get; set; }
    public string? Identifier { get; set; }
    public string? Code { get; set; }
    public string? Value { get; set; }
    public string? Name { get; set; }
    public void Mapping(AutoMapper.Profile profile)
    {
        profile.CreateMap<Generic, GenericDto>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Value));
    }
}
