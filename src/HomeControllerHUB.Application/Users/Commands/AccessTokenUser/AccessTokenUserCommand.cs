using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Domain.Models;
using HomeControllerHUB.Globalization;
using HomeControllerHUB.Infra.Services;
using HomeControllerHUB.Infra.Settings;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeControllerHUB.Application.Users.Commands.AccessTokenUser;

public record AccessTokenUserCommand : IRequest<AccessTokenEntry>
{
    public string Login { get; init; }
    public string Password { get; init; }
}

public class AccessTokenUserCommandHandler : IRequestHandler<AccessTokenUserCommand, AccessTokenEntry>
{
    private readonly IJwtTokenService _jwtService;  
    private readonly ApiUserManager _userManager;
    private readonly ApplicationSettings _applicationSetting;
    private readonly ISharedResource _resource;

    public AccessTokenUserCommandHandler(IJwtTokenService jwtService, ApiUserManager userManager, ApplicationSettings applicationSetting, ISharedResource resource)
    {
        _jwtService = jwtService;
        _userManager = userManager;
        _applicationSetting = applicationSetting;
        _resource = resource;
    }

    public async Task<AccessTokenEntry> Handle(AccessTokenUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.Users
            .Include(x => x.Establishment)
            .FirstOrDefaultAsync(u => u.Login == request.Login, cancellationToken);

        if (user is not null && user.EmailConfirmed && await _userManager.CheckPasswordAsync(user, request.Password))
        {
            Establishment establishment = user.Establishment;

            var jwt = await _jwtService.GenerateAsync(user, establishment, null);
            await _userManager.SetAuthenticationTokenAsync(user, _applicationSetting.JwtSettings.AppName, _applicationSetting.JwtSettings.RefreshTokenName, jwt.RefreshToken);

            return jwt;
        }
        
        throw new AppError(400, _resource.Message("LoginInvalid"));
    }
}