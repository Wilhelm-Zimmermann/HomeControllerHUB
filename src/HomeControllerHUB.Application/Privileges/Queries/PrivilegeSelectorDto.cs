namespace HomeControllerHUB.Application.Privileges.Queries;

public partial class PrivilegeSelectorDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Code { get; set; }
    public string? Domain { get; set; }
    public string? DomainDisplayName { get; set; }
    public string? Action { get; set; }
    public string? ActionDisplayName { get; set; }
    public string? Description { get; set; }
}
