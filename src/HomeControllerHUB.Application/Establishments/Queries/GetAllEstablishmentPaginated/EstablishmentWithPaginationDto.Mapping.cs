using System.Linq.Expressions;
using HomeControllerHUB.Domain.Entities;

namespace HomeControllerHUB.Application.Establishments.Queries.GetAllEstablishmentPaginated;

public partial class EstablishmentWithPaginationDto
{
    public static readonly Expression<Func<Establishment, EstablishmentWithPaginationDto>> Projection = establishment => new EstablishmentWithPaginationDto
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
        Modified = establishment.Modified
    };
}
