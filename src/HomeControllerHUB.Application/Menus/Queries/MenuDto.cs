using HomeControllerHUB.Application.Domains.Queries;
using HomeControllerHUB.Domain.Entities;
namespace HomeControllerHUB.Application.Menus.Queries;

public partial record MenuDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? IconClass { get; set; }
    public string? Link { get; set; }
    public string? Target { get; set; }
    public int Order { get; set; }
    public Guid? ParentId { get; set; }
    public MenuParentDto? Parent { get; set; }
    public Guid? DomainId { get; set; }
    public ApplicationDomainDto? Domain { get; set; }
    public bool Enable { get; set; }
}
