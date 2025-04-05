namespace HomeControllerHUB.Domain.Entities;

public class UserProfile : Base
{
    public Guid UserId { get; set; }
    public virtual ApplicationUser User { get; set; }
    public Guid ProfileId { get; set; }
    public virtual Profile Profile { get; set; }
}