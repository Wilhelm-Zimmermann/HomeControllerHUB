using FluentValidation;
using HomeControllerHUB.Infra.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace HomeControllerHUB.Application.Locations.Commands.CreateLocation;

public class CreateLocationCommandValidator : AbstractValidator<CreateLocationCommand>
{
    private readonly ApplicationDbContext _context;
    
    public CreateLocationCommandValidator(ApplicationDbContext context)
    {
        _context = context;
        
        RuleFor(x => x.EstablishmentId)
            .NotEmpty()
            .MustAsync(EstablishmentExistsAsync).WithMessage("The specified establishment does not exist.");
            
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(255);
            
        RuleFor(x => x.Description)
            .MaximumLength(500);
            
        RuleFor(x => x.Type)
            .IsInEnum();
            
        RuleFor(x => x.ParentLocationId)
            .MustAsync(ParentLocationExistsAsync).WithMessage("The specified parent location does not exist.")
            .When(x => x.ParentLocationId.HasValue);
    }
    
    private async Task<bool> EstablishmentExistsAsync(Guid establishmentId, CancellationToken cancellationToken)
    {
        return await _context.Establishments
            .AnyAsync(e => e.Id == establishmentId, cancellationToken);
    }
    
    private async Task<bool> ParentLocationExistsAsync(Guid? parentLocationId, CancellationToken cancellationToken)
    {
        if (!parentLocationId.HasValue)
            return true;
            
        return await _context.Locations
            .AnyAsync(l => l.Id == parentLocationId.Value, cancellationToken);
    }
} 