using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Models;
using HomeControllerHUB.Globalization;
using HomeControllerHUB.Infra.DatabaseContext;
using HomeControllerHUB.Shared.Common;
using HomeControllerHUB.Shared.Common.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeControllerHUB.Application.Users.Queries.GetUserById;

[Authorize(Domain = DomainNames.User, Action = SecurityActionType.Read)]
public record GetUserByIdQuery(Guid Id) : IRequest<UserDetailDto>;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDetailDto>
{
    private readonly ApplicationDbContext _context;
    private readonly ISharedResource _resource;

    public GetUserByIdQueryHandler(ApplicationDbContext context, ISharedResource resource)
    {
        _context = context;
        _resource = resource;
    }

    public async Task<UserDetailDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .AsNoTracking()
            .Include(u => u.Establishment)
            .Include(u => u.UserEstablishments)
                .ThenInclude(ue => ue.Establishment)
            .Include(u => u.UserProfiles)
                .ThenInclude(up => up.Profile)
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        if (user == null)
            throw new AppError(404, _resource.NotFoundMessage(nameof(ApplicationUser)));

        var establishments = user.UserEstablishments?
            .Select(ue => new UserEstablishmentDto
            {
                EstablishmentId = ue.EstablishmentId,
                Name = ue.Establishment?.Name
            })
            .ToList() ?? [];

        var profiles = user.UserProfiles?
            .Select(up => new UserProfileDto
            {
                ProfileId = up.ProfileId,
                Name = up.Profile?.Name
            })
            .ToList() ?? [];

        return new UserDetailDto
        {
            Id = user.Id,
            Name = user.Name,
            Login = user.Login,
            Email = user.Email,
            Document = user.Document,
            Enable = user.Enable,
            EmailConfirmed = user.EmailConfirmed,
            EstablishmentId = user.EstablishmentId,
            EstablishmentName = user.Establishment?.Name,
            Created = user.Created,
            Modified = user.Modified,
            EstablishmentIds = establishments.Select(e => e.EstablishmentId).ToList(),
            ProfileIds = profiles.Select(p => p.ProfileId).ToList(),
            Establishments = establishments,
            Profiles = profiles
        };
    }
}
