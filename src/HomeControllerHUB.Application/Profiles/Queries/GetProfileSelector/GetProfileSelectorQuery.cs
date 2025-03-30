using AutoMapper;
using AutoMapper.QueryableExtensions;
using HomeControllerHUB.Domain.Models;
using HomeControllerHUB.Infra.DatabaseContext;
using HomeControllerHUB.Shared.Common;
using HomeControllerHUB.Shared.Common.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Profile = HomeControllerHUB.Domain.Entities.Profile;

namespace HomeControllerHUB.Application.Profiles.Queries.GetProfileSelector;

[Authorize(Domain = DomainNames.Establishment, Action = SecurityActionType.Read)]
public record GetProfileSelectorQuery : IRequest<List<ProfileSelectorDto>>
{
}

public class GetProfileSelectorQueryHandler : IRequestHandler<GetProfileSelectorQuery, List<ProfileSelectorDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetProfileSelectorQueryHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<ProfileSelectorDto>> Handle(GetProfileSelectorQuery request, CancellationToken cancellationToken)
    {
        return await _context.Profiles
            .ProjectTo<ProfileSelectorDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }
}