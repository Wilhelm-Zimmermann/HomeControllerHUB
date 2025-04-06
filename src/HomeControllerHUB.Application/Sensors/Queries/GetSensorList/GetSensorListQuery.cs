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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HomeControllerHUB.Application.Sensors.Queries.GetSensorList;

[Authorize(Domain = DomainNames.IoT, Action = SecurityActionType.Read)]
public class GetSensorListQuery : IRequest<List<SensorDto>>
{
    public string? SearchTerm { get; set; }
    public Guid? LocationId { get; set; }
    public bool? IsActive { get; set; }
}

public class GetSensorListQueryValidator : AbstractValidator<GetSensorListQuery>
{
    public GetSensorListQueryValidator()
    {
        // No special validation rules needed
    }
}

public class GetSensorListQueryHandler : IRequestHandler<GetSensorListQuery, List<SensorDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetSensorListQueryHandler(
        ApplicationDbContext context,
        IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<SensorDto>> Handle(GetSensorListQuery request, CancellationToken cancellationToken)
    {
        // Build query
        var query = _context.Sensors
            .Include(s => s.Location)
            .Include(s => s.Establishment)
            .AsQueryable();

        // Apply search filter if provided
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(s => s.Name.ToLower().Contains(searchTerm) || 
                                     s.NormalizedName != null && s.NormalizedName.Contains(searchTerm));
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

        // Map to DTO and materialize
        var sensors = await query
            .ProjectTo<SensorDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return sensors;
    }
} 