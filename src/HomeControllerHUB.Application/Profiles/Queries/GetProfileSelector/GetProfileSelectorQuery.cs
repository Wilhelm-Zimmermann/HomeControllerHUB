using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Infra.DatabaseContext;
using HomeControllerHUB.Shared.Common;
using HomeControllerHUB.Shared.Common.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeControllerHUB.Application.Profiles.Queries.GetProfileSelector;

[Authorize(Domain = DomainNames.Profile, Action = SecurityActionType.Read)]
public record GetProfileSelectorQuery : IRequest<List<ProfileSelectorDto>>
{
}

public class GetProfileSelectorQueryHandler : IRequestHandler<GetProfileSelectorQuery, List<ProfileSelectorDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetProfileSelectorQueryHandler(ApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<List<ProfileSelectorDto>> Handle(GetProfileSelectorQuery request, CancellationToken cancellationToken)
    {
        var establishmentId = _currentUserService.EstablishmentId;

        return await _context.Profiles
            .Where(p => p.EstablishmentId == establishmentId)
            .Select(ProfileSelectorDto.Projection)
            .ToListAsync(cancellationToken);
    }
}
