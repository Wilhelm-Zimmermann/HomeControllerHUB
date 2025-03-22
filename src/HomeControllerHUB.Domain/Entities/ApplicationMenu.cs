using HomeControllerHUB.Shared.Normalize;

namespace HomeControllerHUB.Domain.Entities;

public class ApplicationMenu
{
    public Guid Id { get; set; }
    public Guid? ParentId { get; set; }
    public Guid? DomainId { get; set; }
    public string? Name { get; set; }

    [Normalized(nameof(Name))]
    public string NormalizedName { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Normalized(nameof(Description))]
    public string? NormalizedDescription { get; set; }

    public string? IconClass { get; set; }

    public string? Link { get; set; }

    public string? Target { get; set; }

    public int Order { get; set; }

    public virtual ApplicationMenu Parent { get; set; } = null!;

    public virtual ApplicationDomain Domain { get; set; } = null!;

    public bool Enable { get; set; }
}