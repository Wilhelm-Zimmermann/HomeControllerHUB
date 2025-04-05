using FluentValidation;
using HomeControllerHUB.Infra.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace HomeControllerHUB.Application.Locations.Commands.UpdateLocation;

public class UpdateLocationCommandValidator : AbstractValidator<UpdateLocationCommand>
{
    private readonly ApplicationDbContext _context;
    
    public UpdateLocationCommandValidator(ApplicationDbContext context)
    {
        _context = context;
        
        RuleFor(x => x.Id)
            .NotEmpty()
            .MustAsync(LocationExistsAsync).WithMessage("The specified location does not exist.");
            
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
            
        RuleFor(x => x.ParentLocationId)
            .NotEqual(x => x.Id).WithMessage("A location cannot be its own parent.")
            .When(x => x.ParentLocationId.HasValue);
    }
    
    private async Task<bool> LocationExistsAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Locations
            .AnyAsync(l => l.Id == id, cancellationToken);
    }
    
    private async Task<bool> ParentLocationExistsAsync(Guid? parentLocationId, CancellationToken cancellationToken)
    {
        if (!parentLocationId.HasValue)
            return true;
            
        return await _context.Locations
            .AnyAsync(l => l.Id == parentLocationId.Value, cancellationToken);
    }
} 