using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Domain.Models;
using HomeControllerHUB.Infra.DatabaseContext;
using HomeControllerHUB.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeControllerHUB.Infra.Interceptors;

public class AuthorizationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ApplicationDbContext _context;

    public AuthorizationBehaviour(ICurrentUserService currentUserService, ApplicationDbContext context)
    {
        _currentUserService = currentUserService;
        _context = context;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var authorizeAttributes = request.GetType().GetCustomAttributes(typeof(AuthorizeAttribute), false) as AuthorizeAttribute[];
        
        if(authorizeAttributes.Length == 0) return await next();;

        foreach (var attribute in authorizeAttributes)
        {
            if (attribute.Domain == string.Empty) return await next();
            var isAuthorized = await IsInDomainAndActionAsync(_currentUserService.UserId, attribute.Domain, attribute.Action);
            if(!isAuthorized) throw new AppError(401, "Unauthorized");
        }
        
        return await next();
    }
    
    private async Task<bool> IsInDomainAndActionAsync(Guid? userId, string domainName, string action)
    {
        var user = await _context.Users.SingleOrDefaultAsync(u => u.Id == userId);
        if (user is null)
        {
            return false;
        }

        var domainEntity = await _context.Domains.FirstOrDefaultAsync(k => k.Name == domainName);
        if (domainEntity is null)
        {
            return false;
        }
        
        var userHasAuthorization = await _context.UserProfiles
            .Where(x => x.UserId == userId)
            .SelectMany(up => up.Profile.ProfilePrivileges)
            .AnyAsync(pp => (pp.Privilege.DomainId == domainEntity.Id &&
                             pp.Privilege.Actions == action) || pp.Privilege.NormalizedName == "PLATFORMALL");

        if (userHasAuthorization)
            return true;

        return false;
    }
}