using System.Linq.Expressions;
using HomeControllerHUB.Domain.Entities;

namespace HomeControllerHUB.Application.Generics.Queries;

public partial class GenericDto
{
    public static readonly Expression<Func<Generic, GenericDto>> Projection = generic => new GenericDto
    {
        Id = generic.Id,
        Identifier = generic.Identifier,
        Code = generic.Code,
        Value = generic.Value,
        Name = generic.Value
    };
}
