namespace HomeControllerHUB.Domain.Mappings;

[AttributeUsage(AttributeTargets.Property)]
public class MapFromAttribute : Attribute
{
    public string Name { get; set; }

    public MapFromAttribute(string name)
    {
        Name = name;
    }
}