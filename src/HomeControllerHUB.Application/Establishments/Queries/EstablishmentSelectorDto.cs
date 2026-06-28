namespace HomeControllerHUB.Application.Establishments.Queries;

public class EstablishmentSelectorDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Code { get; set; }
}
