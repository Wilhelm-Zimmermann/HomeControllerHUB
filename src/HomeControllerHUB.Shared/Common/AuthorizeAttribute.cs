using HomeControllerHUB.Shared.Common;

namespace HomeControllerHUB.Shared.Common;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class AuthorizeAttribute : Attribute
{
    public AuthorizeAttribute() {}
    
    public string Domain { get; set; } = string.Empty;

    public string Action { get; set; }
}