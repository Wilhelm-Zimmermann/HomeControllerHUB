using System.Linq.Expressions;
using HomeControllerHUB.Domain.Entities;

namespace HomeControllerHUB.Application.Users.Queries.GetCurrentUser;

public partial class CurrentUserDto
{
    public static readonly Expression<Func<ApplicationUser, CurrentUserDto>> Projection = user => new CurrentUserDto
    {
        Id = user.Id,
        EstablishmentId = user.EstablishmentId,
        Name = user.Name,
        Login = user.Login,
        Email = user.Email,
        Document = user.Document,
        Code = user.Code ?? string.Empty,
        Enable = user.Enable,
        UserProfiles = user.UserProfiles != null
            ? user.UserProfiles.Select(userProfile => new UserProfileDto
            {
                Id = userProfile.Id,
                Profile = new ProfileListDto
                {
                    Id = userProfile.Profile.Id,
                    Name = userProfile.Profile.Name ?? string.Empty,
                    Code = userProfile.Profile.Description ?? string.Empty
                }
            }).ToList()
            : null
    };
}
