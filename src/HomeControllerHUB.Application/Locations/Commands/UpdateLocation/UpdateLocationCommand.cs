using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Models;
using HomeControllerHUB.Globalization;
using HomeControllerHUB.Infra.DatabaseContext;
using HomeControllerHUB.Shared.Common;
using HomeControllerHUB.Shared.Common.Constants;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HomeControllerHUB.Application.Locations.Commands.UpdateLocation;

[Authorize(Domain = DomainNames.Location, Action = SecurityActionType.Update)]
public class UpdateLocationCommand : IRequest
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public LocationType Type { get; set; }
    public Guid? ParentLocationId { get; set; }
}

public class UpdateLocationCommandHandler : IRequestHandler<UpdateLocationCommand>
{
    private readonly ApplicationDbContext _context;
    private readonly ISharedResource _sharedResource;
    
    public UpdateLocationCommandHandler(ApplicationDbContext context, ISharedResource sharedResource)
    {
        _context = context;
        _sharedResource = sharedResource;
    }
    
    public async Task Handle(UpdateLocationCommand request, CancellationToken cancellationToken)
    {
        var location = await _context.Locations
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken);
            
        if (location == null)
        {
            throw new AppError(
                StatusCodes.Status404NotFound,
                _sharedResource.NotFoundMessage(nameof(Location)),
                _sharedResource.Message("TheRequestedLocationCouldNotBeFound"));
        }
        
        // Prevent circular parent references
        if (request.ParentLocationId == request.Id)
        {
            throw new AppError(
                StatusCodes.Status400BadRequest,
                _sharedResource.Message("InvalidParentLocation"),
                _sharedResource.Message("LocationCannotBeItsOwnParent"));
        }
        
        // Update the location
        location.Name = request.Name;
        location.Description = request.Description;
        location.Type = request.Type;
        location.ParentLocationId = request.ParentLocationId;
        
        await _context.SaveChangesAsync(cancellationToken);
    }
} 