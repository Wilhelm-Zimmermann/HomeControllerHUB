using HomeControllerHUB.Domain.Models;
using HomeControllerHUB.Infra.DatabaseContext;
using HomeControllerHUB.Shared.Common;
using HomeControllerHUB.Shared.Common.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeControllerHUB.Application.Users.Queries.GetUserList;

[Authorize(Domain = DomainNames.User, Action = SecurityActionType.Read)]
public record GetUserListQuery : PaginatedRequest<UserListDto>
{
    public bool? Enable { get; init; }
    public Guid? EstablishmentId { get; init; }
}

public class GetUserListQueryHandler : IRequestHandler<GetUserListQuery, PaginatedList<UserListDto>>
{
    private readonly ApplicationDbContext _context;

    public GetUserListQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<UserListDto>> Handle(GetUserListQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Users
            .Include(u => u.UserProfiles)
                .ThenInclude(up => up.Profile)
            .Include(u => u.Establishment)
            .AsNoTracking();

        if (request.Enable.HasValue)
        {
            query = query.Where(u => u.Enable == request.Enable.Value);
        }

        if (request.EstablishmentId.HasValue)
        {
            var establishmentId = request.EstablishmentId.Value;
            query = query.Where(u =>
                u.EstablishmentId == establishmentId ||
                u.UserEstablishments.Any(ue => ue.EstablishmentId == establishmentId));
        }

        if (!string.IsNullOrWhiteSpace(request.SearchBy))
        {
            var searchBy = request.SearchBy.ToLower();
            query = query.Where(u =>
                (u.Name != null && u.Name.ToLower().Contains(searchBy)) ||
                (u.Email != null && u.Email.ToLower().Contains(searchBy)) ||
                (u.Login != null && u.Login.ToLower().Contains(searchBy)) ||
                (u.Document != null && u.Document.ToLower().Contains(searchBy)));
        }

        return await query
            .Select(UserListDto.Projection)
            .PaginateAsync(request, cancellationToken);
    }
}
