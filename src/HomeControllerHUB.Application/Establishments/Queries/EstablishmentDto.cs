namespace HomeControllerHUB.Application.Establishments.Queries;

public partial class EstablishmentDto
{
    public Guid Id { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? SiteName { get; set; }
    public string? Document { get; set; }
    public bool Enable { get; set; } = false;
    public bool IsMaster { get; set; } = false;
    public Guid? SubscriptionPlanId { get; set; }
    public string? SubscriptionPlanName { get; set; }
    public DateTime? SubscriptionEndDate { get; set; }
    public DateTime Created { get; set; }
    public DateTime Modified { get; set; }
    public List<Guid> UserIds { get; set; } = new();
    public List<EstablishmentUserDto> Users { get; set; } = new();
}

public class EstablishmentUserDto
{
    public Guid UserId { get; set; }
    public string? Name { get; set; }
    public string? Login { get; set; }
    public string? Email { get; set; }
}
