using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Mappings;

namespace HomeControllerHUB.Application.Privileges.Queries;

public class PrivilegeSelectorDto : IMapFrom<Privilege>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    [MapFrom(nameof(Privilege.Description))]
    public string Code { get; set; }
}

