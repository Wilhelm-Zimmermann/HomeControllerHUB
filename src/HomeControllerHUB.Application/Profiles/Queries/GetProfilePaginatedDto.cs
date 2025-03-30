using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Domain.Mappings;

namespace HomeControllerHUB.Application.Profiles.Queries;

public class GetProfilePaginatedDto : IMapFrom<Profile>, IPaginatedDto
{
    public Guid Id { get; set; }
    public Guid EstablishmentId { get; set; }
    public string? Name { get; set; }
    public string? NormalizedName { get; set; }
    public string? Description { get; set; }
    public string? NormalizedDescription { get; set; }
    public bool Enable { get; set; }
}