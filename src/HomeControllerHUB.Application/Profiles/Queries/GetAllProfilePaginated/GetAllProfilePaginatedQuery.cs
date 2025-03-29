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

namespace HomeControllerHUB.Application.Profiles.Queries.GetAllProfilePaginated;

[Authorize(Domain = DomainNames.Profile, Action = SecurityActionType.Read)]
public record GetAllProfilePaginatedQuery : PaginatedRequest<GetProfilePaginatedDto>
{
    
}

public class GetAllProfilePaginatedQueryHandler : IRequestHandler<GetAllProfilePaginatedQuery, PaginatedList<GetProfilePaginatedDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetAllProfilePaginatedQueryHandler(ApplicationDbContext context, ICurrentUserService currentUserService, IMapper mapper)
    {
        _context = context;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<PaginatedList<GetProfilePaginatedDto>> Handle(GetAllProfilePaginatedQuery request, CancellationToken cancellationToken)
    {
        var establishmentId = _currentUserService.EstablishmentId;
        var query = _context.Profiles.AsQueryable();

        if (!string.IsNullOrEmpty(request.SearchBy))
        {
            var normalizedSearch = StringExtensions.Normalize(request.SearchBy);
            query = query.Where(x => EF.Functions.Like(x.NormalizedName, $"%{normalizedSearch}%"));
        }

        return await query
            .Where(p => p.EstablishmentId == establishmentId)
            .ProjectTo<GetProfilePaginatedDto>(_mapper.ConfigurationProvider)
            .PaginateAsync(request, cancellationToken);
    }
}