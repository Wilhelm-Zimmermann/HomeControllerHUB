using FluentValidation;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Infra.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace HomeControllerHUB.Application.Sensors.Commands.CreateSensor;

public class CreateSensorCommandValidator : AbstractValidator<CreateSensorCommand>
{
    private readonly ApplicationDbContext _context;
    
    public CreateSensorCommandValidator(ApplicationDbContext context)
    {
        _context = context;
        
        RuleFor(x => x.EstablishmentId)
            .NotEmpty()
            .MustAsync(EstablishmentExistsAsync).WithMessage("The specified establishment does not exist.");
            
        RuleFor(x => x.LocationId)
            .NotEmpty()
            .MustAsync(LocationExistsAsync).WithMessage("The specified location does not exist.");
            
        RuleFor(x => x)
            .MustAsync(LocationBelongsToEstablishmentAsync).WithMessage("The specified location does not belong to the establishment.")
            .When(x => x.EstablishmentId != Guid.Empty && x.LocationId != Guid.Empty);
            
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(255);
            
        RuleFor(x => x.DeviceId)
            .NotEmpty()
            .MaximumLength(100)
            .MustAsync(DeviceIdIsUniqueAsync).WithMessage("The device ID is already in use.");
            
        RuleFor(x => x.Model)
            .NotEmpty()
            .MaximumLength(100);
            
        RuleFor(x => x.FirmwareVersion)
            .MaximumLength(50);
            
        RuleFor(x => x.Type)
            .IsInEnum();
            
        RuleFor(x => x.MaxThreshold)
            .GreaterThan(x => x.MinThreshold)
            .When(x => x.MinThreshold.HasValue && x.MaxThreshold.HasValue)
            .WithMessage("Maximum threshold must be greater than minimum threshold.");
    }
    
    private async Task<bool> EstablishmentExistsAsync(Guid establishmentId, CancellationToken cancellationToken)
    {
        return await _context.Establishments
            .AnyAsync(e => e.Id == establishmentId, cancellationToken);
    }
    
    private async Task<bool> LocationExistsAsync(Guid locationId, CancellationToken cancellationToken)
    {
        return await _context.Locations
            .AnyAsync(l => l.Id == locationId, cancellationToken);
    }
    
    private async Task<bool> LocationBelongsToEstablishmentAsync(CreateSensorCommand command, CancellationToken cancellationToken)
    {
        return await _context.Locations
            .AnyAsync(l => l.Id == command.LocationId && l.EstablishmentId == command.EstablishmentId, cancellationToken);
    }
    
    private async Task<bool> DeviceIdIsUniqueAsync(string deviceId, CancellationToken cancellationToken)
    {
        return !await _context.Sensors
            .AnyAsync(s => s.DeviceId == deviceId, cancellationToken);
    }
} 