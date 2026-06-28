using System.Linq.Expressions;
using HomeControllerHUB.Domain.Entities;

namespace HomeControllerHUB.Application.Privileges.Queries;

public partial class PrivilegeSelectorDto
{
    public static readonly Expression<Func<Privilege, PrivilegeSelectorDto>> Projection = privilege => new PrivilegeSelectorDto
    {
        Id = privilege.Id,
        Name = privilege.Name,
        Code = privilege.Description,
        Domain = privilege.Domain.Name,
        DomainDisplayName = privilege.Domain.Description,
        Action = privilege.Actions,
        ActionDisplayName = privilege.Actions,
        Description = privilege.Description
    };
}
