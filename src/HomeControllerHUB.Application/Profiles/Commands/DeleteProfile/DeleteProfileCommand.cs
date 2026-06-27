using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Interfaces;
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
    private readonly ICurrentUserService _currentUserService;

    public DeleteProfilesCommandHandler(ApplicationDbContext context, ISharedResource resource, ICurrentUserService currentUserService)
    {
        _context = context;
        _resource = resource;
        _currentUserService = currentUserService;
    }

    public async Task Handle(DeleteProfileCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Profiles
            .IgnoreQueryFilters()
            .Where(a => a.Id == request.Id && a.EstablishmentId == _currentUserService.EstablishmentId)
            .FirstOrDefaultAsync(cancellationToken);
        if (entity == null) throw new AppError(404, _resource.NotFoundMessage(nameof(Profile)));

        var userProfilesToDelete = await _context.UserProfiles
            .Where(c => c.Profile.Id == entity.Id)
            .ToListAsync(cancellationToken);

        _context.UserProfiles.RemoveRange(userProfilesToDelete);

        var privilegesToDelete = await _context.ProfilePrivileges
            .Where(c => c.ProfileId == entity.Id)
            .ToListAsync(cancellationToken);

        _context.ProfilePrivileges.RemoveRange(privilegesToDelete);

        _context.Profiles.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
