using HomeControllerHUB.Domain.Entities;
namespace HomeControllerHUB.Application.Profiles.Queries.GetProfileSelector;

public partial class ProfileSelectorDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string? Code { get; set; }
}
