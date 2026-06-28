using HomeControllerHUB.Domain.Interfaces;

namespace HomeControllerHUB.Application.Users.Queries.GetUserList;

public partial class UserListDto : IPaginatedDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Login { get; set; }
    public string? Document { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool Enable { get; set; }
    public Guid EstablishmentId { get; set; }
    public string? EstablishmentName { get; set; }
    public DateTimeOffset Created { get; set; }
    public DateTimeOffset Modified { get; set; }
    public List<Guid> ProfileIds { get; set; } = [];
    public List<UserProfileDto>? UserProfiles { get; set; }
}

public class UserProfileDto
{
    public Guid Id { get; set; }
    public Guid ProfileId { get; set; }
    public string? ProfileName { get; set; }
}
