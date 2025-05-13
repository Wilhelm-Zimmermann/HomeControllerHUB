using AutoMapper;
using HomeControllerHUB.Application.Locations.Queries;
using HomeControllerHUB.Infra.DatabaseContext;
using HomeControllerHUB.Shared.Common;
using HomeControllerHUB.Shared.Common.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeControllerHUB.Application.Locations.Queries.GetLocationList;

[Authorize(Domain = DomainNames.Location, Action = SecurityActionType.Read)]
public class GetLocationListQuery : IRequest<List<LocationDto>>
{
    public Guid? EstablishmentId { get; set; }
    public Guid? ParentLocationId { get; set; }
}

public class GetLocationListQueryHandler : IRequestHandler<GetLocationListQuery, List<LocationDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    
    public GetLocationListQueryHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    
    public async Task<List<LocationDto>> Handle(GetLocationListQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Locations
            .Include(l => l.Establishment)
            .Include(l => l.ParentLocation)
            .AsQueryable();
            
        // Filter by establishment if provided
        if (request.EstablishmentId.HasValue)
        {
            query = query.Where(l => l.EstablishmentId == request.EstablishmentId.Value);
        }
        
        // Filter by parent location if provided
        if (request.ParentLocationId.HasValue)
        {
            query = query.Where(l => l.ParentLocationId == request.ParentLocationId.Value);
        }
        
        // Order by name
        query = query.OrderBy(l => l.Name);
        
        var locations = await query.ToListAsync(cancellationToken);
        var locationDtos = _mapper.Map<List<LocationDto>>(locations);
        
        // Map navigation properties
        foreach (var locationDto in locationDtos)
        {
            var location = locations.First(l => l.Id == locationDto.Id);
            locationDto.EstablishmentName = location.Establishment?.Name ?? "";
            
            if (location.ParentLocation != null)
            {
                locationDto.ParentLocationName = location.ParentLocation.Name;
            }
        }
        
        return locationDtos;
    }
} 