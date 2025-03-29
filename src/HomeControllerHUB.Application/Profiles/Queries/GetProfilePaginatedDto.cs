using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Domain.Mappings;

namespace HomeControllerHUB.Application.Profiles.Queries;

public class GetProfilePaginatedDto : IMapFrom<Profile>, IPaginatedDto
{
    public Guid Id { get; set; }
}