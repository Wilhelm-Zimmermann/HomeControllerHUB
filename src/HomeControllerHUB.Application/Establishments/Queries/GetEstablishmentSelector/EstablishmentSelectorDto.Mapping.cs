using System.Linq.Expressions;
using HomeControllerHUB.Domain.Entities;

namespace HomeControllerHUB.Application.Establishments.Queries.GetEstablishmentSelector;

public partial class EstablishmentSelectorDto
{
    public static readonly Expression<Func<Establishment, EstablishmentSelectorDto>> Projection = establishment => new EstablishmentSelectorDto
    {
        Id = establishment.Id,
        Name = establishment.Name!,
        Code = establishment.Code
    };
}
