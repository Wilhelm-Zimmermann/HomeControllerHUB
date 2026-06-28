using HomeControllerHUB.Domain.Entities;
namespace HomeControllerHUB.Application.Establishments.Queries.GetEstablishmentSelector;

public partial class EstablishmentSelectorDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string? Code { get; set; }
}
