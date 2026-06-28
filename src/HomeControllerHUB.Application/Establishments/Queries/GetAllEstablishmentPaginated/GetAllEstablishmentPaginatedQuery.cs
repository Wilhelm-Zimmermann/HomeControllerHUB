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
    public bool? Enable { get; init; }
}

public class GetAllEstablishmentPaginatedQueryHandler : IRequestHandler<GetAllEstablishmentPaginatedQuery, PaginatedList<EstablishmentWithPaginationDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetAllEstablishmentPaginatedQueryHandler(ApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PaginatedList<EstablishmentWithPaginationDto>> Handle(GetAllEstablishmentPaginatedQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Establishments
            .IgnoreQueryFilters();

        if (request.Enable.HasValue)
        {
            query = query.Where(e => e.Enable == request.Enable.Value);
        }

        if (!string.IsNullOrEmpty(request.SearchBy) && request.SearchBy.Length > 0)
        {
            var normalizedSearch = string.Concat("%", StringExtensions.Normalize(string.Concat(request.SearchBy, string.Empty)), "%");
            query = query.Where(e => EF.Functions.Like(e.NormalizedName, normalizedSearch));
        }

        return await query
            .Select(EstablishmentWithPaginationDto.Projection)
            .PaginateAsync(request, cancellationToken);
    }
}
