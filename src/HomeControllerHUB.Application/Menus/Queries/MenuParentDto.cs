using HomeControllerHUB.Domain.Entities;
namespace HomeControllerHUB.Application.Menus.Queries;

public class MenuParentDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
}
