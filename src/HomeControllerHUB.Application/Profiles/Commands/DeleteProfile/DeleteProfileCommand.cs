using AutoMapper;
using HomeControllerHUB.Domain.Models;
using HomeControllerHUB.Globalization;
using HomeControllerHUB.Infra.DatabaseContext;
using HomeControllerHUB.Shared.Common;
using HomeControllerHUB.Shared.Common.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeControllerHUB.Application.Profiles.Commands.DeleteProfile;

[Authorize(Domain = DomainNames.Profile, Action = SecurityActionType.Delete)]
public record DeleteProfileCommand(Guid Id) : IRequest
{
}

public class DeleteProfilesCommandHandler : IRequestHandler<DeleteProfileCommand>
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

    public async Task Handle(DeleteProfileCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Profiles
            .IgnoreQueryFilters()
            .Where(a => a.Id == request.Id)
            .FirstOrDefaultAsync();
        if(entity == null) throw new AppError(404, _resource.NotFoundMessage(nameof(Profile)));

        var userProfilesToDelete = _context.UserProfiles.Where(c => c.Profile.Id == entity.Id).ToList();

        _context.UserProfiles.RemoveRange(userProfilesToDelete);

        var privilegesToDelete = _context.ProfilePrivileges.Where(c => c.ProfileId == entity.Id).ToList();

        _context.ProfilePrivileges.RemoveRange(privilegesToDelete);

        _context.Profiles.Remove(entity!);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
