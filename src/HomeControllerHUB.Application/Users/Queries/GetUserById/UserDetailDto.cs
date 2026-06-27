using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Mappings;

namespace HomeControllerHUB.Application.Users.Queries.GetUserById;

public class UserDetailDto : IMapFrom<ApplicationUser>
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Login { get; set; }
    public string? Email { get; set; }
    public string? Document { get; set; }
    public bool Enable { get; set; }
    public bool EmailConfirmed { get; set; }
    public Guid EstablishmentId { get; set; }
    public string? EstablishmentName { get; set; }
    public DateTimeOffset Created { get; set; }
    public DateTimeOffset Modified { get; set; }
    public List<Guid> EstablishmentIds { get; set; } = [];
    public List<Guid> ProfileIds { get; set; } = [];
    public List<UserEstablishmentDto> Establishments { get; set; } = [];
    public List<UserProfileDto> Profiles { get; set; } = [];
}

public class UserEstablishmentDto
{
    public Guid EstablishmentId { get; set; }
    public string? Name { get; set; }
}

public class UserProfileDto
{
    public Guid ProfileId { get; set; }
    public string? Name { get; set; }
}
