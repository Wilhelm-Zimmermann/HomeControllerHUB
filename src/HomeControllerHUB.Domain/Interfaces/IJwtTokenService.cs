using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Models;
using Microsoft.IdentityModel.Tokens;

namespace HomeControllerHUB.Domain.Interfaces;

public interface IJwtTokenService
{
    Task<AccessTokenEntry> GenerateAsync(ApplicationUser user, Establishment? establishment,
        string? refreshToken);
    string? GenerateExternalToken();
    ClaimsPrincipal? ValidateToken(string token);
}