using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Infra.DatabaseContext;
using HomeControllerHUB.Shared.Common;
using HomeControllerHUB.Shared.Common.Constants;
using MediatR;

namespace HomeControllerHUB.Application.Locations.Commands.CreateLocation;

[Authorize(Domain = DomainNames.Location, Action = SecurityActionType.Create)]
public class CreateLocationCommand : IRequest<Guid>
{
    public Guid EstablishmentId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public LocationType Type { get; set; }
    public Guid? ParentLocationId { get; set; }
}

public class CreateLocationCommandHandler : IRequestHandler<CreateLocationCommand, Guid>
{
    private readonly ApplicationDbContext _context;
    
    public CreateLocationCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<Guid> Handle(CreateLocationCommand request, CancellationToken cancellationToken)
    {
        var location = new Location
        {
            EstablishmentId = request.EstablishmentId,
            Name = request.Name,
            Description = request.Description,
            Type = request.Type,
            ParentLocationId = request.ParentLocationId
        };
        
        _context.Locations.Add(location);
        await _context.SaveChangesAsync(cancellationToken);
        
        return location.Id;
    }
} 