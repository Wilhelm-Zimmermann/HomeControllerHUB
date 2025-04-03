using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Domain.Mappings;

namespace HomeControllerHUB.Application.Establishments.Queries.GetAllEstablishmentPaginated;

public class EstablishmentWithPaginationDto : IMapFrom<Establishment>, IPaginatedDto
{
    public Guid Id { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? SiteName { get; set; }
    public string Document { get; set; } = null!;
    public bool Enable { get; set; } = false;
    public bool IsMaster { get; set; } = false;
}
