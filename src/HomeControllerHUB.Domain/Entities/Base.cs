namespace HomeControllerHUB.Domain.Entities;

public class Base : IBaseEntity
{
    public Guid Id { get; set; }
    public DateTime Created { get; set; }
    public DateTime Modified { get; set; }

    public Base()
    {
        Id = Guid.NewGuid();
        Created = DateTime.UtcNow;
        Modified = DateTime.UtcNow;
    }
}