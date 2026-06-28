using HomeControllerHUB.Domain.Interfaces;

namespace HomeControllerHUB.Application.Users.Queries.GetCurrentUser;

public partial class CurrentUserDto : IPaginatedDto
{
    public Guid Id { get; set; }
    public Guid EstablishmentId { get; set; }
    public string? Name { get; set; }
    public string? Login { get; set; }
    public string? Email { get; set; }
    public string? Document { get; set; }
    public string Code { get; set; } = null!;
    public bool Enable { get; set; }
    public List<string>? Privileges { get; set; }
    public List<UserProfileDto>? UserProfiles { get; set; }
}

public class UserProfileDto
{
    public Guid Id { get; set; }
    public ProfileListDto Profile { get; set; } = new ProfileListDto();
}

public class ProfileListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
}
