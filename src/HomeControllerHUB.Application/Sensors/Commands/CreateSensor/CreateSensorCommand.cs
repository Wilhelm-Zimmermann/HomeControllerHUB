using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Models;
using HomeControllerHUB.Globalization;
using HomeControllerHUB.Infra.DatabaseContext;
using HomeControllerHUB.Shared.Common;
using HomeControllerHUB.Shared.Common.Constants;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace HomeControllerHUB.Application.Sensors.Commands.CreateSensor;

[Authorize(Domain = DomainNames.Sensor, Action = SecurityActionType.Create)]
public class CreateSensorCommand : IRequest<Guid>
{
    public Guid EstablishmentId { get; set; }
    public Guid LocationId { get; set; }
    public string Name { get; set; } = null!;
    public string DeviceId { get; set; } = null!;
    public SensorType Type { get; set; }
    public string Model { get; set; } = null!;
    public string? FirmwareVersion { get; set; }
    public double? MinThreshold { get; set; }
    public double? MaxThreshold { get; set; }
}

public class CreateSensorCommandHandler : IRequestHandler<CreateSensorCommand, Guid>
{
    private readonly ApplicationDbContext _context;
    private readonly ISharedResource _sharedResource;
    
    public CreateSensorCommandHandler(ApplicationDbContext context, ISharedResource sharedResource)
    {
        _context = context;
        _sharedResource = sharedResource;
    }
    
    public async Task<Guid> Handle(CreateSensorCommand request, CancellationToken cancellationToken)
    {
        // Check if DeviceId is already in use
        if (await _context.Sensors.AnyAsync(s => s.DeviceId == request.DeviceId, cancellationToken))
        {
            throw new AppError(
                StatusCodes.Status400BadRequest,
                _sharedResource.Message("DeviceIdAlreadyInUse"),
                _sharedResource.Message("SensorDeviceIdMustBeUnique"));
        }
        
        // Verify Location exists and belongs to the Establishment
        var location = await _context.Locations
            .FirstOrDefaultAsync(l => l.Id == request.LocationId && l.EstablishmentId == request.EstablishmentId, cancellationToken);
            
        if (location == null)
        {
            throw new AppError(
                StatusCodes.Status404NotFound,
                _sharedResource.NotFoundMessage(nameof(Location)),
                _sharedResource.Message("TheRequestedLocationDoesNotBelongToTheEstablishment"));
        }
        
        // Generate a unique API key for the sensor with deviceId claim
        var apiKey = GenerateApiKey(request.DeviceId);
        
        // Create the sensor
        var sensor = new Sensor
        {
            EstablishmentId = request.EstablishmentId,
            LocationId = request.LocationId,
            Name = request.Name,
            DeviceId = request.DeviceId,
            Type = request.Type,
            Model = request.Model,
            FirmwareVersion = request.FirmwareVersion,
            ApiKey = apiKey,
            MinThreshold = request.MinThreshold,
            MaxThreshold = request.MaxThreshold,
            IsActive = true,
            LastCommunication = DateTime.UtcNow
        };
        
        _context.Sensors.Add(sensor);
        await _context.SaveChangesAsync(cancellationToken);
        
        return sensor.Id;
    }
    
    private string GenerateApiKey(string deviceId)
    {
        var key = new byte[32];
        using (var generator = RandomNumberGenerator.Create())
        {
            generator.GetBytes(key);
        }
        
        return Convert.ToBase64String(key);
    }
} 