using AutoMapper;
using AutoMapper.QueryableExtensions;
using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Domain.Models;
using HomeControllerHUB.Infra.DatabaseContext;
using HomeControllerHUB.Shared.Common;
using HomeControllerHUB.Shared.Common.Constants;
using HomeControllerHUB.Shared.Utils;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeControllerHUB.Application.Establishments.Queries.GetAllEstablishmentPaginated;

[Authorize(Domain = DomainNames.Establishment, Action = SecurityActionType.Read)]
public record GetAllEstablishmentPaginatedQuery : PaginatedRequest<EstablishmentWithPaginationDto>
{
    
}

public class GetAllEstablishmentPaginatedQueryHandler : IRequestHandler<GetAllEstablishmentPaginatedQuery, PaginatedList<EstablishmentWithPaginationDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetAllEstablishmentPaginatedQueryHandler(ApplicationDbContext context, ICurrentUserService currentUserService, IMapper mapper)
    {
        _context = context;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<PaginatedList<EstablishmentWithPaginationDto>> Handle(GetAllEstablishmentPaginatedQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Establishments
            .IgnoreQueryFilters();

        if (!string.IsNullOrEmpty(request.SearchBy) && request.SearchBy.Length > 0)
        {
            var normalizedSearch = string.Concat("%", StringExtensions.Normalize(string.Concat(request.SearchBy, string.Empty)), "%");

            var normalizedQuery1 = query.Where(e => EF.Functions.Like(e.NormalizedName, normalizedSearch));

            query = normalizedQuery1;
        }

        var establishments = await query
            .ProjectTo<EstablishmentWithPaginationDto>(_mapper.ConfigurationProvider)
            .PaginateAsync(request, cancellationToken);

        return establishments;
    }
}