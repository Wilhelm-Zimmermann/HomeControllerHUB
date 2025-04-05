using System.Threading;
using System.Threading.Tasks;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Domain.Models;
using HomeControllerHUB.Globalization;
using HomeControllerHUB.Infra.DatabaseContext;
using HomeControllerHUB.Infra.Services;
using HomeControllerHUB.Infra.Settings;
using HomeControllerHUB.Shared.Common;
using HomeControllerHUB.Shared.Common.Constants;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HomeControllerHUB.Application.Users.Commands.RefreshToken;

public record RefreshTokenCommand : IRequest<AccessTokenEntry>
{
    public string RefreshToken { get; init; } = string.Empty;
}

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AccessTokenEntry>
{
    private readonly ApplicationDbContext _context;
    private readonly IJwtTokenService _jwtService;
    private readonly ApiUserManager _userManager;
    private readonly ApplicationSettings _applicationSettings;
    private readonly ISharedResource _resource;
    
    public RefreshTokenCommandHandler(
        ApplicationDbContext context,
        IJwtTokenService jwtService,
        ApiUserManager userManager,
        ApplicationSettings applicationSettings,
        ISharedResource resource)
    {
        _context = context;
        _jwtService = jwtService;
        _userManager = userManager;
        _applicationSettings = applicationSettings;
        _resource = resource;
    }
    
    public async Task<AccessTokenEntry> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // Find the user with the refresh token
        var user = await _userManager.Users
            .Include(x => x.Establishment)
            .FirstOrDefaultAsync(u => 
                _userManager.GetAuthenticationTokenAsync(u, _applicationSettings.JwtSettings.AppName, 
                    _applicationSettings.JwtSettings.RefreshTokenName).Result == request.RefreshToken, 
                cancellationToken);
        
        if (user == null || !user.EmailConfirmed || !user.Enable)
            throw new AppError(400, _resource.Message("InvalidRefreshToken"));
        
        // Generate new tokens
        var jwt = await _jwtService.GenerateAsync(user, user.Establishment, null);
        
        // Update refresh token
        await _userManager.SetAuthenticationTokenAsync(user, _applicationSettings.JwtSettings.AppName, 
            _applicationSettings.JwtSettings.RefreshTokenName, jwt.RefreshToken);
        
        return jwt;
    }
} 