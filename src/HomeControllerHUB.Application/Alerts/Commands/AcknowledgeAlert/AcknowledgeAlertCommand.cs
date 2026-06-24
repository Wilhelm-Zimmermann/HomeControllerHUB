using FluentValidation;
using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Domain.Models;
using HomeControllerHUB.Globalization;
using HomeControllerHUB.Infra.DatabaseContext;
using HomeControllerHUB.Shared.Common;
using HomeControllerHUB.Shared.Common.Constants;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HomeControllerHUB.Application.Alerts.Commands.AcknowledgeAlert;

[Authorize(Domain = DomainNames.IoT, Action = SecurityActionType.Update)]
public class AcknowledgeAlertCommand : IRequest
{
    public Guid Id { get; set; }
}

public class AcknowledgeAlertCommandValidator : AbstractValidator<AcknowledgeAlertCommand>
{
    public AcknowledgeAlertCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public class AcknowledgeAlertCommandHandler : IRequestHandler<AcknowledgeAlertCommand>
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ISharedResource _sharedResource;

    public AcknowledgeAlertCommandHandler(
        ApplicationDbContext context,
        ICurrentUserService currentUserService,
        ISharedResource sharedResource)
    {
        _context = context;
        _currentUserService = currentUserService;
        _sharedResource = sharedResource;
    }

    public async Task Handle(AcknowledgeAlertCommand request, CancellationToken cancellationToken)
    {
        var alert = await _context.SensorAlerts
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (alert == null)
        {
            throw new AppError(
                StatusCodes.Status404NotFound,
                _sharedResource.NotFoundMessage("Alert"),
                _sharedResource.Message("AlertNotFound"));
        }

        if (alert.IsAcknowledged)
        {
            return;
        }

        alert.IsAcknowledged = true;
        alert.AcknowledgedAt = DateTime.UtcNow;
        alert.AcknowledgedById = _currentUserService.UserId;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
