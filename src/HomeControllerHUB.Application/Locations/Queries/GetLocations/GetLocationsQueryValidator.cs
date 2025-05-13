using FluentValidation;
using HomeControllerHUB.Infra.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace HomeControllerHUB.Application.Locations.Queries.GetLocations;

public class GetLocationsQueryValidator : AbstractValidator<GetLocationsQuery>
{
    private readonly ApplicationDbContext _context;
    
    public GetLocationsQueryValidator(ApplicationDbContext context)
    {
        _context = context;
        
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1);
            
        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1)
            .LessThanOrEqualTo(100);
            
        RuleFor(x => x.EstablishmentId)
            .MustAsync(EstablishmentExistsAsync)
            .WithMessage("The specified establishment does not exist.")
            .When(x => x.EstablishmentId.HasValue);
            
        RuleFor(x => x.ParentLocationId)
            .MustAsync(ParentLocationExistsAsync)
            .WithMessage("The specified parent location does not exist.")
            .When(x => x.ParentLocationId.HasValue);
    }
    
    private async Task<bool> EstablishmentExistsAsync(Guid? establishmentId, CancellationToken cancellationToken)
    {
        if (!establishmentId.HasValue)
            return true;
            
        return await _context.Establishments
            .AnyAsync(e => e.Id == establishmentId.Value, cancellationToken);
    }
    
    private async Task<bool> ParentLocationExistsAsync(Guid? parentLocationId, CancellationToken cancellationToken)
    {
        if (!parentLocationId.HasValue)
            return true;
            
        return await _context.Locations
            .AnyAsync(l => l.Id == parentLocationId.Value, cancellationToken);
    }
} 