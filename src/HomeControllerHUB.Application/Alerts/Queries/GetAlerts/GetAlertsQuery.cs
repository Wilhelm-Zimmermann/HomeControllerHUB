using FluentValidation;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Models;
using HomeControllerHUB.Infra.DatabaseContext;
using HomeControllerHUB.Shared.Common;
using HomeControllerHUB.Shared.Common.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeControllerHUB.Application.Alerts.Queries.GetAlerts;

[Authorize(Domain = DomainNames.IoT, Action = SecurityActionType.Read)]
public class GetAlertsQuery : IRequest<PaginatedList<AlertListDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Search { get; set; }
    public Guid? SensorId { get; set; }
    public Guid? LocationId { get; set; }
    public Guid? EstablishmentId { get; set; }
    public string? Type { get; set; }
    public bool? IsAcknowledged { get; set; }
    public DateTime? CreatedStart { get; set; }
    public DateTime? CreatedEnd { get; set; }
}

public class GetAlertsQueryValidator : AbstractValidator<GetAlertsQuery>
{
    public GetAlertsQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThan(0);
        RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(100);

        When(x => !string.IsNullOrWhiteSpace(x.Type), () =>
        {
            RuleFor(x => x.Type)
                .Must(BeValidAlertType)
                .WithMessage("Invalid alert type");
        });

        When(x => x.CreatedStart.HasValue && x.CreatedEnd.HasValue, () =>
        {
            RuleFor(x => x.CreatedStart!.Value)
                .LessThanOrEqualTo(x => x.CreatedEnd!.Value)
                .WithMessage("Created start must be before or equal to created end");
        });
    }

    private static bool BeValidAlertType(string? type)
    {
        return Enum.TryParse<AlertType>(type, true, out _);
    }
}

public class GetAlertsQueryHandler : IRequestHandler<GetAlertsQuery, PaginatedList<AlertListDto>>
{
    private readonly ApplicationDbContext _context;

    public GetAlertsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<AlertListDto>> Handle(GetAlertsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.SensorAlerts
            .AsNoTracking()
            .AsQueryable();

        if (request.SensorId.HasValue)
        {
            query = query.Where(a => a.SensorId == request.SensorId.Value);
        }

        if (request.LocationId.HasValue)
        {
            query = query.Where(a => a.Sensor.LocationId == request.LocationId.Value);
        }

        if (request.EstablishmentId.HasValue)
        {
            query = query.Where(a => a.Sensor.EstablishmentId == request.EstablishmentId.Value);
        }

        if (request.IsAcknowledged.HasValue)
        {
            query = query.Where(a => a.IsAcknowledged == request.IsAcknowledged.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Type))
        {
            if (!Enum.TryParse<AlertType>(request.Type.Trim(), true, out var alertType))
            {
                return PaginatedList<AlertListDto>.Empty<AlertListDto>(request.PageNumber, request.PageSize);
            }

            query = query.Where(a => a.Type == alertType);
        }

        if (request.CreatedStart.HasValue)
        {
            query = query.Where(a => a.Created >= request.CreatedStart.Value);
        }

        if (request.CreatedEnd.HasValue)
        {
            query = query.Where(a => a.Created <= request.CreatedEnd.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            var normalizedSearch = search.ToLower();
            var hasAlertTypeFilter = Enum.TryParse<AlertType>(search, true, out var searchAlertType);

            query = query.Where(a =>
                a.Message.ToLower().Contains(normalizedSearch) ||
                a.Sensor.Name.ToLower().Contains(normalizedSearch) ||
                a.Sensor.DeviceId.ToLower().Contains(normalizedSearch) ||
                a.Sensor.Location.Name.ToLower().Contains(normalizedSearch) ||
                (a.Sensor.Establishment.Name != null && a.Sensor.Establishment.Name.ToLower().Contains(normalizedSearch)) ||
                (a.AcknowledgedBy != null && a.AcknowledgedBy.Name != null && a.AcknowledgedBy.Name.ToLower().Contains(normalizedSearch)) ||
                (hasAlertTypeFilter && a.Type == searchAlertType));
        }

        var projectedQuery = query
            .OrderByDescending(a => a.Timestamp)
            .Select(a => new AlertListDto
            {
                Id = a.Id,
                SensorId = a.SensorId,
                SensorName = a.Sensor.Name,
                SensorDeviceId = a.Sensor.DeviceId,
                SensorType = a.Sensor.Type,
                LocationId = a.Sensor.LocationId,
                LocationName = a.Sensor.Location.Name,
                EstablishmentId = a.Sensor.EstablishmentId,
                EstablishmentName = a.Sensor.Establishment.Name,
                Type = a.Type,
                Message = a.Message,
                MinThreshold = a.Sensor.MinThreshold,
                MaxThreshold = a.Sensor.MaxThreshold,
                IsAcknowledged = a.IsAcknowledged,
                AcknowledgedAt = a.AcknowledgedAt,
                AcknowledgedById = a.AcknowledgedById,
                AcknowledgedByName = a.AcknowledgedBy != null ? a.AcknowledgedBy.Name : null,
                Timestamp = a.Timestamp,
                Created = a.Created
            });

        return await PaginatedList<AlertListDto>.CreateAsync(
            projectedQuery,
            request.PageNumber,
            request.PageSize,
            cancellationToken);
    }
}
