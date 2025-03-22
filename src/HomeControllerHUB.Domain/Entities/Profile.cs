namespace HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Shared.Normalize;

public class Profile : Base
{
    public Guid EstablishmentId { get; set; }
    public virtual Establishment Establishment { get; set; } = null!;
    public string? Name { get; set; }
    [Normalized(nameof(Name))]
    public string? NormalizedName { get; set; }
    public string? Description { get; set; }
    [Normalized(nameof(Description))]
    public string? NormalizedDescription { get; set; }
    public bool Enable { get; set; }
    public virtual IList<UserProfile> UserProfiles { get; private set; } = new List<UserProfile>();
    public virtual IList<ProfilePrivilege> ProfilePrivileges { get; private set; } = new List<ProfilePrivilege>();
}