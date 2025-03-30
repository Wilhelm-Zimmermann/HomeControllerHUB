using AutoMapper;
using AutoMapper.QueryableExtensions;
using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Globalization;
using HomeControllerHUB.Infra.DatabaseContext;
using HomeControllerHUB.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeControllerHUB.Application.Generics.Queries.GenericSelector;

[Authorize]
public record GenericSelectorQuery(string Identifier) : IRequest<List<GenericDto>>
{
}

public class GenericSelectorQueryHandler : IRequestHandler<GenericSelectorQuery, List<GenericDto>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ISharedResource _resource;

    public GenericSelectorQueryHandler(ICurrentUserService currentUserService, ApplicationDbContext context, IMapper mapper, ISharedResource resource)
    {
        _currentUserService = currentUserService;
        _context = context;
        _mapper = mapper;
        _resource = resource;
    }

    public async Task<List<GenericDto>> Handle(GenericSelectorQuery request, CancellationToken cancellationToken)
    {
        return await _context.Generics
            .Where(p => p.Identifier == request.Identifier)
            .ProjectTo<GenericDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
