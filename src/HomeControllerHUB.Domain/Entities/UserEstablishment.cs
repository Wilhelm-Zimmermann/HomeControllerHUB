namespace HomeControllerHUB.Domain.Entities;

public class UserEstablishment : Base
{
    public Guid UserId { get; set; }
    public virtual ApplicationUser User { get; set; } = new ApplicationUser();

    public Guid EstablishmentId { get; set; }
    public virtual Establishment Establishment { get; set; } = new Establishment();
}