using HomeControllerHUB.Infra.DatabaseContext;
using HomeControllerHUB.Shared.Common;
using HomeControllerHUB.Shared.Common.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeControllerHUB.Application.Establishments.Queries.GetEstablishmentSelector;

[Authorize(Domain = DomainNames.Establishment, Action = SecurityActionType.Read)]
public record GetEstablishmentSelectorQuery : IRequest<List<EstablishmentSelectorDto>>
{
}

public class GetEstablishmentSelectorQueryHandler : IRequestHandler<GetEstablishmentSelectorQuery, List<EstablishmentSelectorDto>>
{
    private readonly ApplicationDbContext _context;

    public GetEstablishmentSelectorQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<EstablishmentSelectorDto>> Handle(GetEstablishmentSelectorQuery request, CancellationToken cancellationToken)
    {
        return await _context.Establishments
            .Select(EstablishmentSelectorDto.Projection)
            .ToListAsync(cancellationToken);
    }
}
