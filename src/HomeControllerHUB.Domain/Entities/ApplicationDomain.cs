using HomeControllerHUB.Shared.Normalize;

namespace HomeControllerHUB.Domain.Entities;

public class ApplicationDomain
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    [Normalized(nameof(Name))]
    public string? NormalizedName { get; set; }
    public string? Description { get; set; }
    [Normalized(nameof(Description))]
    public string? NormalizedDescription { get; set; }
    public bool Enable { get; set; } = true;
}