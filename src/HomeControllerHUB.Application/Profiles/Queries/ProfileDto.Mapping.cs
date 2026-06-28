using System.Linq.Expressions;
using HomeControllerHUB.Domain.Entities;

namespace HomeControllerHUB.Application.Profiles.Queries;

public partial class ProfileDto
{
    public static readonly Expression<Func<Profile, ProfileDto>> Projection = profile => new ProfileDto
    {
        Id = profile.Id,
        EstablishmentId = profile.EstablishmentId,
        Name = profile.Name,
        NormalizedName = profile.NormalizedName,
        Description = profile.Description,
        NormalizedDescription = profile.NormalizedDescription,
        Enable = profile.Enable,
        PrivilegeIds = profile.ProfilePrivileges.Select(profilePrivilege => profilePrivilege.PrivilegeId).ToList(),
        Privileges = profile.ProfilePrivileges.Select(profilePrivilege => new ProfilePrivilegeDto
        {
            PrivilegeId = profilePrivilege.PrivilegeId,
            Domain = profilePrivilege.Privilege.Domain.Name,
            DomainDisplayName = profilePrivilege.Privilege.Domain.Description,
            Action = profilePrivilege.Privilege.Actions,
            ActionDisplayName = profilePrivilege.Privilege.Actions,
            Description = profilePrivilege.Privilege.Description
        }).ToList()
    };
}
