using FluentValidation;
using HomeControllerHUB.Infra.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace HomeControllerHUB.Application.Locations.Commands.DeleteLocation;

public class DeleteLocationCommandValidator : AbstractValidator<DeleteLocationCommand>
{
    private readonly ApplicationDbContext _context;
    
    public DeleteLocationCommandValidator(ApplicationDbContext context)
    {
        _context = context;
        
        RuleFor(x => x.Id)
            .NotEmpty()
            .MustAsync(LocationExistsAsync).WithMessage("The specified location does not exist.");
    }
    
    private async Task<bool> LocationExistsAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Locations
            .AnyAsync(l => l.Id == id, cancellationToken);
    }
} 