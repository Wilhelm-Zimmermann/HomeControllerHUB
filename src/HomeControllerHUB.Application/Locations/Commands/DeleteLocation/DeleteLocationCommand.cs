using HomeControllerHUB.Domain.Models;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Globalization;
using HomeControllerHUB.Infra.DatabaseContext;
using HomeControllerHUB.Shared.Common;
using HomeControllerHUB.Shared.Common.Constants;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HomeControllerHUB.Application.Locations.Commands.DeleteLocation;

[Authorize(Domain = DomainNames.Location, Action = SecurityActionType.Delete)]
public class DeleteLocationCommand : IRequest
{
    public Guid Id { get; set; }
}

public class DeleteLocationCommandHandler : IRequestHandler<DeleteLocationCommand>
{
    private readonly ApplicationDbContext _context;
    private readonly ISharedResource _sharedResource;
    
    public DeleteLocationCommandHandler(ApplicationDbContext context, ISharedResource sharedResource)
    {
        _context = context;
        _sharedResource = sharedResource;
    }
    
    public async Task Handle(DeleteLocationCommand request, CancellationToken cancellationToken)
    {
        var location = await _context.Locations
            .Include(l => l.ChildLocations)
            .Include(l => l.Sensors)
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken);
            
        if (location == null)
        {
            throw new AppError(
                StatusCodes.Status404NotFound,
                _sharedResource.NotFoundMessage(nameof(Location)),
                _sharedResource.Message("TheRequestedLocationCouldNotBeFound"));
        }
        
        // Check if location has child locations
        if (location.ChildLocations.Any())
        {
            throw new AppError(
                StatusCodes.Status400BadRequest,
                _sharedResource.Message("LocationHasChildren"),
                _sharedResource.Message("DeleteAllChildLocationsFirst"));
        }
        
        // Check if location has sensors
        if (location.Sensors.Any())
        {
            throw new AppError(
                StatusCodes.Status400BadRequest,
                _sharedResource.Message("LocationHasSensors"),
                _sharedResource.Message("MoveSensorsBeforeDeleting"));
        }
        
        _context.Locations.Remove(location);
        await _context.SaveChangesAsync(cancellationToken);
    }
} 