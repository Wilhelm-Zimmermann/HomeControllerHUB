using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Domain.Mappings;

namespace HomeControllerHUB.Application.Domains.Queries;
public class ApplicationDomainDto : IMapFrom<ApplicationDomain>, IPaginatedDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool Enable { get; set; } = true;

}
