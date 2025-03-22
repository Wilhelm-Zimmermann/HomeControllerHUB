using Microsoft.AspNetCore.Identity;
using HomeControllerHUB.Shared.Normalize;
namespace HomeControllerHUB.Domain.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public Guid EstablishmentId { get; set; }
    public virtual Establishment? Establishment { get; set; }
    public string? Name { get; set; }
    [Normalized(nameof(Name))]
    public string? NormalizedName { get; set; }
    [Normalized(nameof(Name))]
    public string? NormalizedUserName { get; set; }
    public string? Code { get; set; }
    public string? Login { get; set; }
    public string? Document { get; set; }
    public bool Enable { get; set; }
    public DateTimeOffset Created { get; set; }
    public Guid? CreatedBy { get; set; }
    public string CreatedByUserName { get; set; }
    public DateTimeOffset? LastModified { get; set; }
    public Guid? LastModifiedBy { get; set; }
    public string LastModifiedByUserName { get; set; }
    public virtual IList<UserEstablishment> UserEstablishments { get; private set; } = new List<UserEstablishment>();
    public virtual IList<UserProfile>? UserProfiles { get; private set; }
}