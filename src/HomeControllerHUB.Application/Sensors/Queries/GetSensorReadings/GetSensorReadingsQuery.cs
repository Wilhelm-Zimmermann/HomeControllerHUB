using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using HomeControllerHUB.Application.Sensors.Queries;
using HomeControllerHUB.Domain.Models;
using HomeControllerHUB.Globalization;
using HomeControllerHUB.Infra.DatabaseContext;
using HomeControllerHUB.Shared.Common;
using HomeControllerHUB.Shared.Common.Constants;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HomeControllerHUB.Application.Sensors.Queries.GetSensorReadings;

[Authorize(Domain = DomainNames.IoT, Action = SecurityActionType.Read)]
public class GetSensorReadingsQuery : IRequest<PaginatedList<SensorReadingDto>>
{
    public Guid SensorId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class GetSensorReadingsQueryValidator : AbstractValidator<GetSensorReadingsQuery>
{
    public GetSensorReadingsQueryValidator()
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

public class GetSensorReadingsQueryHandler : IRequestHandler<GetSensorReadingsQuery, PaginatedList<SensorReadingDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ISharedResource _sharedResource;

    public GetSensorReadingsQueryHandler(
        ApplicationDbContext context,
        IMapper mapper,
        ISharedResource sharedResource)
    {
        _context = context;
        _mapper = mapper;
        _sharedResource = sharedResource;
    }

    public async Task<PaginatedList<SensorReadingDto>> Handle(GetSensorReadingsQuery request, CancellationToken cancellationToken)
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
        var query = _context.SensorReadings
            .Where(r => r.SensorId == request.SensorId)
            .AsQueryable();

        // Apply date filters if provided
        if (request.StartDate.HasValue)
        {
            query = query.Where(r => r.Timestamp >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(r => r.Timestamp <= request.EndDate.Value);
        }

        // Order by newest first
        query = query.OrderByDescending(r => r.Timestamp);

        // Apply pagination and map to DTO
        var paginatedReadings = await PaginatedList<SensorReadingDto>.CreateAsync(
            query.ProjectTo<SensorReadingDto>(_mapper.ConfigurationProvider),
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        return paginatedReadings;
    }
} 