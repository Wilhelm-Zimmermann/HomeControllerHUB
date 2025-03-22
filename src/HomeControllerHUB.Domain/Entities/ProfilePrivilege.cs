namespace HomeControllerHUB.Domain.Entities;

public class ProfilePrivilege : Base
{
    public Guid ProfileId { get; set; }
    public virtual Profile Profile { get; set; } = null!;

    public Guid PrivilegeId { get; set; }
    public virtual Privilege Privilege { get; set; } = null!;
}