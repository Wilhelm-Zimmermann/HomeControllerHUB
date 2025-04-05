using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Domain.Mappings;

namespace HomeControllerHUB.Application.Users.Queries.GetCurrentUser;

public class CurrentUserDto : IMapFrom<ApplicationUser>, IPaginatedDto
{
    public Guid Id { get; set; }
    public Guid EstablishmentId { get; set; }

    [MapFrom(nameof(ApplicationUser.Name))]
    public string? Name { get; set; }
    public string? Login { get; set; }
    public string? Email { get; set; }
    public string? Document { get; set; }
    public string Code { get; set; } = null!;
    [MapFrom(nameof(ApplicationUser.Enable))]
    public bool Enable { get; set; }
    public List<string>? Privileges { get; set; }
    public List<UserProfileDto>? UserProfiles { get; set; }
}

public class UserProfileDto : IMapFrom<UserProfile>
{
    public Guid Id { get; set; }
    public ProfileListDto Profile { get; set; } = new ProfileListDto();
}

public class ProfileListDto : IMapFrom<Profile>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    [MapFrom(nameof(ApplicationMenu.Description))]
    public string Code { get; set; } = null!;
}