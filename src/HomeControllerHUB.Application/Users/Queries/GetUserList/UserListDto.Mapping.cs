using System.Linq.Expressions;
using HomeControllerHUB.Domain.Entities;

namespace HomeControllerHUB.Application.Users.Queries.GetUserList;

public partial class UserListDto
{
    public static readonly Expression<Func<ApplicationUser, UserListDto>> Projection = user => new UserListDto
    {
        Id = user.Id,
        Name = user.Name,
        Email = user.Email,
        Login = user.Login,
        Document = user.Document,
        EmailConfirmed = user.EmailConfirmed,
        Enable = user.Enable,
        EstablishmentId = user.EstablishmentId,
        EstablishmentName = user.Establishment != null ? user.Establishment.Name : null,
        Created = user.Created,
        Modified = user.Modified,
        ProfileIds = user.UserProfiles != null ? user.UserProfiles.Select(userProfile => userProfile.ProfileId).ToList() : new List<Guid>(),
        UserProfiles = user.UserProfiles != null
            ? user.UserProfiles.Select(userProfile => new UserProfileDto
            {
                Id = userProfile.Id,
                ProfileId = userProfile.ProfileId,
                ProfileName = userProfile.Profile != null ? userProfile.Profile.Name : null
            }).ToList()
            : null
    };
}
