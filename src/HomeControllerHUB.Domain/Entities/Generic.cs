using HomeControllerHUB.Shared.Normalize;

namespace HomeControllerHUB.Domain.Entities;

public class Generic : Base
{
    public string Identifier { get; set; }
    public string? Code { get; set; }
    public string? Value { get; set; }
    [Normalized(nameof(Value))]
    public string? NormalizedValue { get; set; }
    public int? DisplayOrder { get; set; }
    public bool Enable { get; set; }
}