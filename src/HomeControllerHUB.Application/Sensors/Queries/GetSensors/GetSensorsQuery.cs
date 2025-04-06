using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using HomeControllerHUB.Domain.Models;
using HomeControllerHUB.Globalization;
using HomeControllerHUB.Infra.DatabaseContext;
using HomeControllerHUB.Shared.Common;
using HomeControllerHUB.Shared.Common.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HomeControllerHUB.Application.Sensors.Queries.GetSensors;

[Authorize(Domain = DomainNames.IoT, Action = SecurityActionType.Read)]
public class GetSensorsQuery : IRequest<PaginatedList<SensorDto>>
{
    public string? SearchTerm { get; set; }
    public Guid? LocationId { get; set; }
    public bool? IsActive { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class GetSensorsQueryValidator : AbstractValidator<GetSensorsQuery>
{
    public GetSensorsQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThan(0);
        RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(100);
    }
}

public class GetSensorsQueryHandler : IRequestHandler<GetSensorsQuery, PaginatedList<SensorDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetSensorsQueryHandler(
        ApplicationDbContext context,
        IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedList<SensorDto>> Handle(GetSensorsQuery request, CancellationToken cancellationToken)
    {
        // Build query
        var query = _context.Sensors
            .Include(s => s.Location)
            .AsQueryable();

        // Apply search filter if provided
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(s => s.Name.ToLower().Contains(searchTerm));
        }

        // Apply location filter if provided
        if (request.LocationId.HasValue)
        {
            query = query.Where(s => s.LocationId == request.LocationId.Value);
        }

        // Apply active filter if provided
        if (request.IsActive.HasValue)
        {
            query = query.Where(s => s.IsActive == request.IsActive.Value);
        }

        // Order by name
        query = query.OrderBy(s => s.Name);

        // Apply pagination and map to DTO
        var paginatedSensors = await PaginatedList<SensorDto>.CreateAsync(
            query.ProjectTo<SensorDto>(_mapper.ConfigurationProvider),
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        return paginatedSensors;
    }
} 