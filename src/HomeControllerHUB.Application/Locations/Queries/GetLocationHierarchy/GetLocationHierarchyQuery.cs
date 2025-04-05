using AutoMapper;
using HomeControllerHUB.Application.Locations.Queries;
using HomeControllerHUB.Infra.DatabaseContext;
using HomeControllerHUB.Shared.Common;
using HomeControllerHUB.Shared.Common.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeControllerHUB.Application.Locations.Queries.GetLocationHierarchy;

[Authorize(Domain = DomainNames.Location, Action = SecurityActionType.Read)]
public class GetLocationHierarchyQuery : IRequest<List<LocationHierarchyDto>>
{
    public Guid? EstablishmentId { get; set; }
}

public class GetLocationHierarchyQueryHandler : IRequestHandler<GetLocationHierarchyQuery, List<LocationHierarchyDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    
    public GetLocationHierarchyQueryHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    
    public async Task<List<LocationHierarchyDto>> Handle(GetLocationHierarchyQuery request, CancellationToken cancellationToken)
    {
        // Get all locations for the establishment
        var allLocations = await _context.Locations
            .Where(l => !request.EstablishmentId.HasValue || l.EstablishmentId == request.EstablishmentId.Value)
            .ToListAsync(cancellationToken);
            
        // Map all locations to DTOs
        var locationDtos = _mapper.Map<List<LocationHierarchyDto>>(allLocations);
        
        // Create a dictionary for quick lookup
        var dtoLookup = locationDtos.ToDictionary(dto => dto.Id);
        
        // Create the hierarchy structure
        foreach (var dto in locationDtos)
        {
            if (dto.ParentLocationId.HasValue && dtoLookup.TryGetValue(dto.ParentLocationId.Value, out var parentDto))
            {
                parentDto.Children.Add(dto);
            }
        }
        
        // Return only root locations (those without a parent or with a parent that doesn't exist in our set)
        return locationDtos
            .Where(dto => !dto.ParentLocationId.HasValue || !dtoLookup.ContainsKey(dto.ParentLocationId.Value))
            .OrderBy(dto => dto.Name)
            .ToList();
    }
} 