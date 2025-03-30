using AutoMapper;
using AutoMapper.QueryableExtensions;
using HomeControllerHUB.Domain.Models;
using HomeControllerHUB.Infra.DatabaseContext;
using HomeControllerHUB.Shared.Common;
using HomeControllerHUB.Shared.Common.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Profile = HomeControllerHUB.Domain.Entities.Profile;

namespace HomeControllerHUB.Application.Establishments.Queries.GetEstablishmentSelector;

[Authorize(Domain = DomainNames.Establishment, Action = SecurityActionType.Read)]
public record GetEstablishmentSelectorQuery : IRequest<List<EstablishmentSelectorDto>>
{
}

public class GetEstablishmentSelectorQueryHandler : IRequestHandler<GetEstablishmentSelectorQuery, List<EstablishmentSelectorDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetEstablishmentSelectorQueryHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<EstablishmentSelectorDto>> Handle(GetEstablishmentSelectorQuery request, CancellationToken cancellationToken)
    {
        return await _context.Establishments
            .ProjectTo<EstablishmentSelectorDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }
}