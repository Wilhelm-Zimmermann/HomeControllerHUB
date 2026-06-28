using HomeControllerHUB.Domain.Interfaces;

namespace HomeControllerHUB.Application.Profiles.Queries;

public partial class GetProfilePaginatedDto : IPaginatedDto
{
    public Guid Id { get; set; }
    public Guid EstablishmentId { get; set; }
    public string? Name { get; set; }
    public string? NormalizedName { get; set; }
    public string? Description { get; set; }
    public string? NormalizedDescription { get; set; }
    public bool Enable { get; set; }
    public int UsersCount { get; set; }
    public int PrivilegesCount { get; set; }
    public DateTime Created { get; set; }
    public DateTime Modified { get; set; }
}
