using AutoMapper;
using AutoMapper.QueryableExtensions;
using HomeControllerHUB.Infra.DatabaseContext;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeControllerHUB.Application.Profiles.Queries.GetProfileById;

public record GetProfileByIdQuery(Guid Id) : IRequest<ProfileDto>
{
}

public class GetProfileByIdQueryHandler : IRequestHandler<GetProfileByIdQuery, ProfileDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetProfileByIdQueryHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ProfileDto> Handle(GetProfileByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Profiles
            .Where(p => p.Id == request.Id)
            .ProjectTo<ProfileDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
    }
}