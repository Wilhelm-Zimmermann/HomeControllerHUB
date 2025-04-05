using AutoMapper;
using HomeControllerHUB.Application.Locations.Queries;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Models;
using HomeControllerHUB.Globalization;
using HomeControllerHUB.Infra.DatabaseContext;
using HomeControllerHUB.Shared.Common;
using HomeControllerHUB.Shared.Common.Constants;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HomeControllerHUB.Application.Locations.Queries.GetLocation;

[Authorize(Domain = DomainNames.Location, Action = SecurityActionType.Read)]
public class GetLocationQuery : IRequest<LocationDto>
{
    public Guid Id { get; set; }
}

public class GetLocationQueryHandler : IRequestHandler<GetLocationQuery, LocationDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ISharedResource _sharedResource;
    
    public GetLocationQueryHandler(
        ApplicationDbContext context,
        IMapper mapper,
        ISharedResource sharedResource)
    {
        _context = context;
        _mapper = mapper;
        _sharedResource = sharedResource;
    }
    
    public async Task<LocationDto> Handle(GetLocationQuery request, CancellationToken cancellationToken)
    {
        var location = await _context.Locations
            .Include(l => l.Establishment)
            .Include(l => l.ParentLocation)
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken);
            
        if (location == null)
        {
            throw new AppError(
                StatusCodes.Status404NotFound,
                _sharedResource.NotFoundMessage(nameof(Location)),
                _sharedResource.Message("TheRequestedLocationCouldNotBeFound"));
        }
        
        var locationDto = _mapper.Map<LocationDto>(location);
        
        // Map navigation properties
        locationDto.EstablishmentName = location.Establishment?.Name ?? "";
        
        if (location.ParentLocation != null)
        {
            locationDto.ParentLocationName = location.ParentLocation.Name;
        }
        
        return locationDto;
    }
} 