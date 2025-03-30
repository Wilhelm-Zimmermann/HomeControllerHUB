
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Models;
using HomeControllerHUB.Globalization;
using HomeControllerHUB.Infra.DatabaseContext;
using HomeControllerHUB.Shared.Common;
using HomeControllerHUB.Shared.Common.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Profile = AutoMapper.Profile;

namespace HomeControllerHUB.Application.Profiles.Commands.UpdateProfile;

[Authorize(Domain = DomainNames.Profile, Action = SecurityActionType.Update)]
public record UpdateProfileCommand : IRequest
{
    public Guid Id { get; init; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public bool Enable { get; set; }
    public List<Guid> PrivilegeIds { get; set; } = new List<Guid>();
}

public class UpdateProfilesCommandHandler : IRequestHandler<UpdateProfileCommand>
{
    private readonly ApplicationDbContext _context;
    private readonly ISharedResource _resource;

    public UpdateProfilesCommandHandler(ApplicationDbContext context, ISharedResource resource)
    {
        _context = context;
        _resource = resource;
    }

    public async Task Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var profile = await _context.Profiles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
        if(profile == null) throw new AppError(404, _resource.NotFoundMessage(nameof(Profile)));


        profile.Name = request.Name;
        profile.Description = request.Description;
        profile.Enable = request.Enable;

        var privilegesToDelete = _context.ProfilePrivileges.Where(c => c.ProfileId == profile.Id).ToList();

        _context.ProfilePrivileges.RemoveRange(privilegesToDelete);

        foreach (var privilegeId in request.PrivilegeIds)
        {
            var privilege = await _context.Privilege.FindAsync(new object[] { privilegeId }, cancellationToken);
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
