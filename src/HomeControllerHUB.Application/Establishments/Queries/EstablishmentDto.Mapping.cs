using System.Linq.Expressions;
using HomeControllerHUB.Domain.Entities;

namespace HomeControllerHUB.Application.Establishments.Queries;

public partial class EstablishmentDto
{
    public static readonly Expression<Func<Establishment, EstablishmentDto>> Projection = establishment => new EstablishmentDto
    {
        Id = establishment.Id,
        Code = establishment.Code,
        Name = establishment.Name,
        SiteName = establishment.SiteName,
        Document = establishment.Document,
        Enable = establishment.Enable,
        IsMaster = establishment.IsMaster,
        SubscriptionPlanId = establishment.SubscriptionPlanId,
        SubscriptionPlanName = establishment.SubscriptionPlan != null ? establishment.SubscriptionPlan.Name : null,
        SubscriptionEndDate = establishment.SubscriptionEndDate,
        Created = establishment.Created,
        Modified = establishment.Modified,
        UserIds = establishment.UserEstablishments.Select(userEstablishment => userEstablishment.UserId).ToList(),
        Users = establishment.UserEstablishments.Select(userEstablishment => new EstablishmentUserDto
        {
            UserId = userEstablishment.UserId,
            Name = userEstablishment.User.Name,
            Login = userEstablishment.User.Login,
            Email = userEstablishment.User.Email
        }).ToList()
    };
}
