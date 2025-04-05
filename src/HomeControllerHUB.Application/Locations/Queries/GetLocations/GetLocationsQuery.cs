using AutoMapper;
using HomeControllerHUB.Application.Locations.Queries;
using HomeControllerHUB.Domain.Models;
using HomeControllerHUB.Infra.DatabaseContext;
using HomeControllerHUB.Shared.Common;
using HomeControllerHUB.Shared.Common.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeControllerHUB.Application.Locations.Queries.GetLocations;

[Authorize(Domain = DomainNames.Location, Action = SecurityActionType.Read)]
public class GetLocationsQuery : IRequest<PaginatedList<LocationDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public Guid? EstablishmentId { get; set; }
    public Guid? ParentLocationId { get; set; }
    public string? SearchString { get; set; }
}

public class GetLocationsQueryHandler : IRequestHandler<GetLocationsQuery, PaginatedList<LocationDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    
    public GetLocationsQueryHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    
    public async Task<PaginatedList<LocationDto>> Handle(GetLocationsQuery request, CancellationToken cancellationToken)
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
        else
        {
            // If no parent is specified, show root locations (those without a parent)
            query = query.Where(l => l.ParentLocationId == null);
        }
        
        // Filter by search string if provided
        if (!string.IsNullOrEmpty(request.SearchString))
        {
            var normalizedSearchString = request.SearchString.ToUpper();
            query = query.Where(l => l.NormalizedName!.Contains(normalizedSearchString) || 
                                     (l.NormalizedDescription != null && l.NormalizedDescription.Contains(normalizedSearchString)));
        }
        
        // Order by name
        query = query.OrderBy(l => l.Name);
        
        var locations = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
            
        var totalCount = await query.CountAsync(cancellationToken);
        
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
        
        return new PaginatedList<LocationDto>(
            locationDtos, 
            totalCount, 
            request.PageNumber, 
            request.PageSize);
    }
} 