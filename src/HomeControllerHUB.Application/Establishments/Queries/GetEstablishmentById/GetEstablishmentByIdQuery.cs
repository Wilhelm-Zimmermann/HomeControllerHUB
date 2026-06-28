using HomeControllerHUB.Infra.DatabaseContext;
using HomeControllerHUB.Shared.Common;
using HomeControllerHUB.Shared.Common.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeControllerHUB.Application.Establishments.Queries.GetEstablishmentById;

[Authorize(Domain = DomainNames.Establishment, Action = SecurityActionType.Read)]
public record GetEstablishmentByIdQuery(Guid Id) : IRequest<EstablishmentDto>
{
}

public class GetEstablishmentByIdQueryHandler : IRequestHandler<GetEstablishmentByIdQuery, EstablishmentDto>
{
    private readonly ApplicationDbContext _context;

    public GetEstablishmentByIdQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EstablishmentDto> Handle(GetEstablishmentByIdQuery request, CancellationToken cancellationToken)
    {
        var establishment = await _context.Establishments
            .Where(p => p.Id == request.Id)
            .Select(EstablishmentDto.Projection)
            .FirstOrDefaultAsync(cancellationToken);

        return establishment!;
    }
}
