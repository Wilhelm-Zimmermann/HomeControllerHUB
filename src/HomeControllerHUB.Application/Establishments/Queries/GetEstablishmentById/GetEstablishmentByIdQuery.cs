using AutoMapper;
using AutoMapper.QueryableExtensions;
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
    private readonly IMapper _mapper;

    public GetEstablishmentByIdQueryHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<EstablishmentDto> Handle(GetEstablishmentByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Establishments
            .Where(p => p.Id == request.Id)
            .ProjectTo<EstablishmentDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
    }
}