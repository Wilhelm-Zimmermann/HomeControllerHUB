using AutoMapper;
using AutoMapper.QueryableExtensions;
using HomeControllerHUB.Application.Establishments.Queries;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Domain.Models;
using HomeControllerHUB.Globalization;
using HomeControllerHUB.Infra.DatabaseContext;
using HomeControllerHUB.Infra.Services;
using HomeControllerHUB.Shared.Common;
using HomeControllerHUB.Shared.Common.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeControllerHUB.Application.Users.Queries.GetCurrentUser;

[Authorize(Domain = DomainNames.User, Action = SecurityActionType.Read)]
public record GetCurrentUserQuery : IRequest<CurrentUserDto>
{
}

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, CurrentUserDto>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ApiUserManager _userManager;
    private readonly ISharedResource _resource;

    public GetCurrentUserQueryHandler(ICurrentUserService currentUserService, ApplicationDbContext context,
        IMapper mapper, ApiUserManager userManager, ISharedResource resource)
    {
        _currentUserService = currentUserService;
        _context = context;
        _mapper = mapper;
        _userManager = userManager;
        _resource = resource;
    }

    public async Task<CurrentUserDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        if (currentUserId == null) throw new AppError(404, _resource.NotFoundMessage(nameof(ApplicationUser)));

        var user = _context.Users.FirstOrDefault(u => u.Id == currentUserId);
        if (user == null) throw new AppError(404, _resource.NotFoundMessage(nameof(ApplicationUser)));

        var record = await _context.Users
            .AsNoTracking()
            .Where(k => k.Id == currentUserId)
            .ProjectTo<CurrentUserDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        bool admin = false;
        List<string> userPrivileges = new();

        if (record!.UserProfiles != null)
        {
            foreach (var userProfile in record.UserProfiles)
            {
                var profilePrivileges = _context.ProfilePrivileges.Where(c => c.ProfileId == userProfile.Profile.Id)
                    .Include(x => x.Profile)
                    .Include(x => x.Privilege)
                    .ToList();

                foreach (var profilePrivilege in profilePrivileges)
                {
                    userPrivileges.Add(profilePrivilege.Privilege.Name);

                    if (profilePrivilege.Privilege.Name == "platform-all") admin = true;
                }
            }
        }

        record.Privileges = userPrivileges.Count > 0 ? userPrivileges : new List<string>();

        if (admin)
        {
            record.Establishments = _context.Establishments
                .Where(e => e.Enable == true)
                .ProjectTo<EstablishmentSelectorDto>(_mapper.ConfigurationProvider)
                .ToList();

            return record;
        }

        record.Establishments = _context.Establishments
            .Where(p => p.UserEstablishments != null && p.UserEstablishments.Any(r => r.UserId == currentUserId))
            .Where(e => e.Enable == true)
            .ProjectTo<EstablishmentSelectorDto>(_mapper.ConfigurationProvider)
            .ToList();

        return record;
    }
}