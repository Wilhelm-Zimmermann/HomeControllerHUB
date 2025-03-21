namespace HomeControllerHUB.Domain.Entities;

public class Base
{
    public Guid Id { get; set; }
    public DateTime Created { get; set; }
    public DateTime Modified { get; set; }

    public Base(Guid id, DateTime created, DateTime modified)
    {
        Id = id;
        Created = created;
        Modified = modified;
    }
}