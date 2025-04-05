using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using HomeControllerHUB.Application.Sensors.Queries;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Models;
using HomeControllerHUB.Globalization;
using HomeControllerHUB.Infra.DatabaseContext;
using HomeControllerHUB.Shared.Common;
using HomeControllerHUB.Shared.Common.Constants;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HomeControllerHUB.Application.Sensors.Queries.GetSensorAlerts;

[Authorize(Domain = DomainNames.IoT, Action = SecurityActionType.Read)]
public class GetSensorAlertsQuery : IRequest<PaginatedList<SensorAlertDto>>
{
    public Guid SensorId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool? IsAcknowledged { get; set; }
    public AlertType? AlertType { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class GetSensorAlertsQueryValidator : AbstractValidator<GetSensorAlertsQuery>
{
    public GetSensorAlertsQueryValidator()
    {
        RuleFor(x => x.SensorId).NotEmpty();
        RuleFor(x => x.PageNumber).GreaterThan(0);
        RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(100);
        
        // Validate date range if both are provided
        When(x => x.StartDate.HasValue && x.EndDate.HasValue, () => {
            RuleFor(x => x.StartDate!.Value).LessThan(x => x.EndDate!.Value)
                .WithMessage("Start date must be before end date");
        });
    }
}

public class GetSensorAlertsQueryHandler : IRequestHandler<GetSensorAlertsQuery, PaginatedList<SensorAlertDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ISharedResource _sharedResource;

    public GetSensorAlertsQueryHandler(
        ApplicationDbContext context,
        IMapper mapper,
        ISharedResource sharedResource)
    {
        _context = context;
        _mapper = mapper;
        _sharedResource = sharedResource;
    }

    public async Task<PaginatedList<SensorAlertDto>> Handle(GetSensorAlertsQuery request, CancellationToken cancellationToken)
    {
        // First verify sensor exists
        var sensorExists = await _context.Sensors
            .AnyAsync(s => s.Id == request.SensorId, cancellationToken);

        if (!sensorExists)
        {
            throw new AppError(
                StatusCodes.Status404NotFound,
                _sharedResource.NotFoundMessage("Sensor"),
                _sharedResource.Message("SensorNotFound"));
        }

        // Build query
        var query = _context.SensorAlerts
            .Where(a => a.SensorId == request.SensorId)
            .AsQueryable();

        // Apply filters if provided
        if (request.StartDate.HasValue)
        {
            query = query.Where(a => a.Timestamp >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(a => a.Timestamp <= request.EndDate.Value);
        }

        if (request.IsAcknowledged.HasValue)
        {
            query = query.Where(a => a.IsAcknowledged == request.IsAcknowledged.Value);
        }

        if (request.AlertType.HasValue)
        {
            query = query.Where(a => a.Type == request.AlertType.Value);
        }

        // Order by newest first
        query = query.OrderByDescending(a => a.Timestamp);

        // Apply pagination and map to DTO
        var paginatedAlerts = await PaginatedList<SensorAlertDto>.CreateAsync(
            query.ProjectTo<SensorAlertDto>(_mapper.ConfigurationProvider),
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        return paginatedAlerts;
    }
} 