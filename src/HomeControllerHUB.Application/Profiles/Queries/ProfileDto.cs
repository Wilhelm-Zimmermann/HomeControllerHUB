namespace HomeControllerHUB.Application.Profiles.Queries;

public partial class ProfileDto
{
    public Guid Id { get; set; }
    public Guid EstablishmentId { get; set; }
    public string? Name { get; set; }
    public string? NormalizedName { get; set; }
    public string? Description { get; set; }
    public string? NormalizedDescription { get; set; }
    public bool Enable { get; set; }
    public List<Guid> PrivilegeIds { get; set; } = [];
    public List<ProfilePrivilegeDto> Privileges { get; set; } = [];
}

public class ProfilePrivilegeDto
{
    public Guid PrivilegeId { get; set; }
    public string? Domain { get; set; }
    public string? DomainDisplayName { get; set; }
    public string? Action { get; set; }
    public string? ActionDisplayName { get; set; }
    public string? Description { get; set; }
}
