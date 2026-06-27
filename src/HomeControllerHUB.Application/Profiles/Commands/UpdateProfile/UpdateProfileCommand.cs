
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Domain.Models;
using HomeControllerHUB.Globalization;
using HomeControllerHUB.Infra.DatabaseContext;
using HomeControllerHUB.Shared.Common;
using HomeControllerHUB.Shared.Common.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Profile = HomeControllerHUB.Domain.Entities.Profile;

namespace HomeControllerHUB.Application.Profiles.Commands.UpdateProfile;

[Authorize(Domain = DomainNames.Profile, Action = SecurityActionType.Update)]
public record UpdateProfileCommand : IRequest
{
    public Guid Id { get; init; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public bool Enable { get; set; }
    public List<Guid>? PrivilegeIds { get; set; }
}

public class UpdateProfilesCommandHandler : IRequestHandler<UpdateProfileCommand>
{
    private readonly ApplicationDbContext _context;
    private readonly ISharedResource _resource;
    private readonly ICurrentUserService _currentUserService;

    public UpdateProfilesCommandHandler(ApplicationDbContext context, ISharedResource resource, ICurrentUserService currentUserService)
    {
        _context = context;
        _resource = resource;
        _currentUserService = currentUserService;
    }

    public async Task Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var profile = await _context.Profiles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.Id == request.Id && r.EstablishmentId == _currentUserService.EstablishmentId, cancellationToken);
        if(profile == null) throw new AppError(404, _resource.NotFoundMessage(nameof(Profile)));


        profile.Name = request.Name;
        profile.Description = request.Description;
        profile.Enable = request.Enable;

        if (request.PrivilegeIds == null)
        {
            await _context.SaveChangesAsync(cancellationToken);
            return;
        }

        var privilegesToDelete = await _context.ProfilePrivileges
            .Where(c => c.ProfileId == profile.Id)
            .ToListAsync(cancellationToken);

        _context.ProfilePrivileges.RemoveRange(privilegesToDelete);

        foreach (var privilegeId in request.PrivilegeIds.Distinct())
        {
            var privilege = await _context.Privilege.FirstOrDefaultAsync(p => p.Id == privilegeId && p.EstablishmentId == profile.EstablishmentId, cancellationToken);
            if(privilege == null) throw new AppError(404, _resource.NotFoundMessage(nameof(Privilege)));

            var profilePrivilege = new ProfilePrivilege()
            {
                Profile = profile,
                PrivilegeId = privilegeId,
            };

            _context.ProfilePrivileges.Add(profilePrivilege);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
