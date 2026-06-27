using AutoMapper;
using AutoMapper.QueryableExtensions;
using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Infra.DatabaseContext;
using HomeControllerHUB.Shared.Common;
using HomeControllerHUB.Shared.Common.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeControllerHUB.Application.Profiles.Queries.GetProfileById;

[Authorize(Domain = DomainNames.Profile, Action = SecurityActionType.Read)]
public record GetProfileByIdQuery(Guid Id) : IRequest<ProfileDto>
{
}

public class GetProfileByIdQueryHandler : IRequestHandler<GetProfileByIdQuery, ProfileDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public GetProfileByIdQueryHandler(ApplicationDbContext context, IMapper mapper, ICurrentUserService currentUserService)
    {
        _context = context;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<ProfileDto> Handle(GetProfileByIdQuery request, CancellationToken cancellationToken)
    {
        var establishmentId = _currentUserService.EstablishmentId;

        return await _context.Profiles
            .Where(p => p.Id == request.Id && p.EstablishmentId == establishmentId)
            .ProjectTo<ProfileDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
