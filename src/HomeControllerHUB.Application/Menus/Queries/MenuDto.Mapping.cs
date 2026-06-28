using System.Linq.Expressions;
using HomeControllerHUB.Application.Domains.Queries;
using HomeControllerHUB.Domain.Entities;

namespace HomeControllerHUB.Application.Menus.Queries;

public partial record MenuDto
{
    public static readonly Expression<Func<ApplicationMenu, MenuDto>> Projection = menu => new MenuDto
    {
        Id = menu.Id,
        Name = menu.Name,
        Description = menu.Description,
        IconClass = menu.IconClass,
        Link = menu.Link,
        Target = menu.Target,
        Order = menu.Order,
        ParentId = menu.ParentId,
        Parent = menu.ParentId.HasValue
            ? new MenuParentDto
            {
                Id = menu.Parent.Id,
                Name = menu.Parent.Name,
                Description = menu.Parent.Description
            }
            : null,
        DomainId = menu.DomainId,
        Domain = menu.DomainId.HasValue
            ? new ApplicationDomainDto
            {
                Id = menu.Domain.Id,
                Name = menu.Domain.Name,
                Description = menu.Domain.Description
            }
            : null,
        Enable = menu.Enable
    };
}
