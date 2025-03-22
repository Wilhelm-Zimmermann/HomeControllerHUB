namespace HomeControllerHUB.Domain.Entities;

public class Base
{
    public Guid Id { get; set; }
    public DateTime Created { get; set; }
    public DateTime Modified { get; set; }

    public Base()
    {
        Id = Guid.NewGuid();
        Created = DateTime.Now;
        Modified = DateTime.Now;
    }
}