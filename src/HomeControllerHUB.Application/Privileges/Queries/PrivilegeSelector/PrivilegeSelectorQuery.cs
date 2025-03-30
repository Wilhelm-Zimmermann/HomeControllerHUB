using AutoMapper;
using AutoMapper.QueryableExtensions;
using HomeControllerHUB.Application.Users.Queries.GetCurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;
using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Globalization;
using HomeControllerHUB.Infra.DatabaseContext;
using HomeControllerHUB.Infra.Services;
using HomeControllerHUB.Shared.Common;
using HomeControllerHUB.Shared.Common.Constants;
using HomeControllerHUB.Shared.Utils;

namespace HomeControllerHUB.Application.Privileges.Queries.PrivilegeSelector;

[Authorize(Domain = DomainNames.Privilege, Action = SecurityActionType.Read)]
public record PrivilegeSelectorQuery(string? SearchBy) : IRequest<List<PrivilegeSelectorDto>>
{
}

public class PrivilegeSelectorQueryHandler : IRequestHandler<PrivilegeSelectorQuery, List<PrivilegeSelectorDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly ApiUserManager _userManager;
    private readonly ISharedResource _resource;
    private readonly IMediator _mediator;

    public PrivilegeSelectorQueryHandler(ApplicationDbContext context, IMapper mapper, ICurrentUserService currentUserService, ApiUserManager userManager, ISharedResource resource, IMediator mediator)
    {
        _context = context;
        _mapper = mapper;
        _currentUserService = currentUserService;
        _userManager = userManager;
        _resource = resource;
        _mediator = mediator;
    }

    public async Task<List<PrivilegeSelectorDto>> Handle(PrivilegeSelectorQuery request, CancellationToken cancellationToken)
    {
        var entity = _context.Privilege
            .IgnoreQueryFilters();

        var query = new GetCurrentUserQuery();
        var currentUser = await _mediator.Send(query);

        if (currentUser.Privileges == null)
        {
            return new List<PrivilegeSelectorDto>();
        }

        if (!string.IsNullOrEmpty(request.SearchBy) && request.SearchBy.Length > 0)
        {
            entity = entity.Where(p => EF.Functions.Like(p.NormalizedName, string.Concat("%", StringExtensions.Normalize(request.SearchBy), "%")));
        }

        var result = await entity
            .ProjectTo<PrivilegeSelectorDto>(_mapper.ConfigurationProvider)
            .OrderBy(c => c.Name)
            .ToListAsync();

        var privileges = GetAccessiblePrivileges(currentUser.Privileges, result);

        return privileges;
    }

    static List<PrivilegeSelectorDto> GetAccessiblePrivileges(List<string> myPrivileges, List<PrivilegeSelectorDto> privileges)
    {
        if (myPrivileges.Contains("platform-all"))
        {
            return privileges;
        }

        return privileges.Where(privilege =>
        {
            if (myPrivileges.Contains(privilege.Name))
            {
                return true;
            }

            var privilegeGroup = privilege.Name.Split('-').FirstOrDefault();
            var allPrivilege = $"{privilegeGroup}-all";

            return myPrivileges.Contains(allPrivilege);
        }).ToList();
    }
}
