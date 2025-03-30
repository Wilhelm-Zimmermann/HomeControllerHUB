using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Mappings;

namespace HomeControllerHUB.Application.Establishments.Queries.GetEstablishmentSelector;

public class EstablishmentSelectorDto : IMapFrom<Establishment>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string? Code { get; set; }
}