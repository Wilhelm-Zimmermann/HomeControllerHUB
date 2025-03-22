using HomeControllerHUB.Shared.Normalize;

namespace HomeControllerHUB.Domain.Entities;

public class Privilege : Base
{
    public string Name { get; set; } = null!;

    [Normalized(nameof(Name))]
    public string? NormalizedName { get; set; }

    public string? Description { get; set; }

    [Normalized(nameof(Description))]
    public string? NormalizedDescription { get; set; }

    public string? Actions { get; set; }

    public bool Enable { get; set; } = true;

    public Guid DomainId { get; set; }

    public virtual ApplicationDomain Domain { get; set; } = null!;   
    public Guid EstablishmentId { get; set; }

    public virtual Establishment Establishment { get; set; } = null!;
}