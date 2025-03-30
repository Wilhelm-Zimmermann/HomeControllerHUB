using AutoMapper;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Models;
using HomeControllerHUB.Globalization;
using HomeControllerHUB.Infra.DatabaseContext;
using HomeControllerHUB.Shared.Common;
using HomeControllerHUB.Shared.Common.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeControllerHUB.Application.Establishments.Commands.DeleteEstablishment;

[Authorize(Domain = DomainNames.Establishment, Action = SecurityActionType.Delete)]
public record DeleteEstablishmentCommand(Guid Id) : IRequest
{
}

public class DeleteProfilesCommandHandler : IRequestHandler<DeleteEstablishmentCommand>
{
    private readonly ApplicationDbContext _context;
    private readonly ISharedResource _resource;
    private readonly IMediator _mediator;

    public DeleteProfilesCommandHandler(ApplicationDbContext context, ISharedResource resource, IMediator mediator)
    {
        _context = context;
        _resource = resource;
        _mediator = mediator;
    }

    public async Task Handle(DeleteEstablishmentCommand request, CancellationToken cancellationToken)
    {
        var establishment = await _context.Establishments
            .IgnoreQueryFilters()
            .Where(a => a.Id == request.Id)
            .FirstOrDefaultAsync();
        if(establishment == null) throw new AppError(404, _resource.NotFoundMessage(nameof(Establishment)));

        establishment.Enable = false;

        var usersToDelete = _context.UserEstablishments.Where(c => c.EstablishmentId == establishment.Id).ToList();
        _context.UserEstablishments.RemoveRange(usersToDelete);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
