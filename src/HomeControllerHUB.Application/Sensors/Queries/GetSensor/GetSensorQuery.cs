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

namespace HomeControllerHUB.Application.Sensors.Queries.GetSensor;

[Authorize(Domain = DomainNames.IoT, Action = SecurityActionType.Read)]
public class GetSensorQuery : IRequest<SensorDto>
{
    public Guid Id { get; set; }
}

public class GetSensorQueryValidator : AbstractValidator<GetSensorQuery>
{
    public GetSensorQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public class GetSensorQueryHandler : IRequestHandler<GetSensorQuery, SensorDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ISharedResource _sharedResource;

    public GetSensorQueryHandler(
        ApplicationDbContext context,
        IMapper mapper,
        ISharedResource sharedResource)
    {
        _context = context;
        _mapper = mapper;
        _sharedResource = sharedResource;
    }

    public async Task<SensorDto> Handle(GetSensorQuery request, CancellationToken cancellationToken)
    {
        var sensor = await _context.Sensors
            .Include(s => s.Location)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (sensor == null)
        {
            throw new AppError(
                StatusCodes.Status404NotFound,
                _sharedResource.NotFoundMessage("Sensor"),
                _sharedResource.Message("SensorNotFound"));
        }

        return _mapper.Map<SensorDto>(sensor);
    }
} 