namespace HomeControllerHUB.Shared.Normalize;

[AttributeUsage(AttributeTargets.Property)]
public class NormalizedAttribute : Attribute
{
    public string PropertyName { get; }

    public NormalizedAttribute(string propertyName)
    {
        PropertyName = propertyName;
    }
}