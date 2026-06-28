using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Domain.Models;
using HomeControllerHUB.Infra.DatabaseContext;
using HomeControllerHUB.Shared.Common;
using HomeControllerHUB.Shared.Common.Constants;
using HomeControllerHUB.Shared.Utils;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeControllerHUB.Application.Profiles.Queries.GetAllProfilePaginated;

[Authorize(Domain = DomainNames.Profile, Action = SecurityActionType.Read)]
public record GetAllProfilePaginatedQuery : PaginatedRequest<GetProfilePaginatedDto>
{
    public bool? Enable { get; init; }
}

public class GetAllProfilePaginatedQueryHandler : IRequestHandler<GetAllProfilePaginatedQuery, PaginatedList<GetProfilePaginatedDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetAllProfilePaginatedQueryHandler(ApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PaginatedList<GetProfilePaginatedDto>> Handle(GetAllProfilePaginatedQuery request, CancellationToken cancellationToken)
    {
        var establishmentId = _currentUserService.EstablishmentId;
        var query = _context.Profiles.AsQueryable();

        if (!string.IsNullOrEmpty(request.SearchBy))
        {
            var normalizedSearch = StringExtensions.Normalize(request.SearchBy);
            query = query.Where(x =>
                EF.Functions.Like(x.NormalizedName, $"%{normalizedSearch}%") ||
                EF.Functions.Like(x.NormalizedDescription, $"%{normalizedSearch}%"));
        }

        if (request.Enable.HasValue)
        {
            query = query.Where(x => x.Enable == request.Enable.Value);
        }

        return await query
            .Where(p => p.EstablishmentId == establishmentId)
            .Select(GetProfilePaginatedDto.Projection)
            .PaginateAsync(request, cancellationToken);
    }
}
