using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Domain.Models;
using HomeControllerHUB.Globalization;
using HomeControllerHUB.Infra.DatabaseContext;
using HomeControllerHUB.Infra.Services;
using HomeControllerHUB.Shared.Common;
using HomeControllerHUB.Shared.Utils;

namespace HomeControllerHUB.Application.Menus.Queries.MenuSelector;

[Authorize]
public record MenuSelectorQuery(string? SearchBy) : IRequest<List<MenuDto>>
{
}

public class MenuSelectorQueryHandler : IRequestHandler<MenuSelectorQuery, List<MenuDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly ApiUserManager _userManager;
    private readonly ISharedResource _resource;

    public MenuSelectorQueryHandler(ApplicationDbContext context, IMapper mapper, ICurrentUserService currentUserService, ApiUserManager userManager, ISharedResource resource)
    {
        _context = context;
        _mapper = mapper;
        _currentUserService = currentUserService;
        _userManager = userManager;
        _resource = resource;
    }

    public async Task<List<MenuDto>> Handle(MenuSelectorQuery request, CancellationToken cancellationToken)
    {
        var user = _context.Users
            .Include(x => x.UserProfiles)
            .FirstOrDefault(u => u.Id == _currentUserService.UserId);
        if (user == null) throw new AppError(404, _resource.NotFoundMessage(nameof(ApplicationUser)));
        
        var privileges = await GetPrivileges(user.Id);

        var query = _context.Menus
            .IgnoreQueryFilters();

        if (!privileges.admin)
        {
            query = query.Where(m => (m.DomainId.HasValue && privileges.domains.Contains(m.DomainId.Value)) || m.DomainId == null);
        }

        if (!string.IsNullOrEmpty(request.SearchBy) && request.SearchBy.Length > 0)
        {
            query = query.Where(p => EF.Functions.Like(p.NormalizedName, string.Concat("%", StringExtensions.Normalize(request.SearchBy), "%")));
        }

        var menus = await query
            .ProjectTo<MenuDto>(_mapper.ConfigurationProvider)
            .OrderBy(c => c.Order)
            .ToListAsync();

        if (!privileges.admin)
        {
            for (int i = menus.Count - 1; i >= 0; i--)
            {
                if (menus[i].DomainId == null && menus[i].Description != "Início")
                {
                    bool hasChildMenu = menus.Any(menu => menu.ParentId == menus[i].Id);
                    if (!hasChildMenu)
                    {
                        menus.RemoveAt(i);
                    }
                }
            }
        }

        return menus;
    }
    
    private async Task<(List<Guid> domains, bool admin)> GetPrivileges(Guid userId)
    {
        var admin = await _context.UserProfiles
            .Where(x => x.UserId == userId)
            .SelectMany(up => up.Profile.ProfilePrivileges)
            .AnyAsync(pp => pp.Privilege.NormalizedName == "PLATFORMALL");

        var domains = await _context.UserProfiles
            .Where(x => x.UserId == userId)
            .SelectMany(up => up.Profile.ProfilePrivileges)
            .Select(x => x.Privilege.DomainId)
            .Distinct()
            .ToListAsync();

        return (domains, admin);
    }
}
