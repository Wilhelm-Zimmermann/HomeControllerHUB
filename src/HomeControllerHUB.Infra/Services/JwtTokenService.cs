using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Domain.Models;
using HomeControllerHUB.Infra.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace HomeControllerHUB.Infra.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly ApplicationSettings _siteSetting;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public JwtTokenService(ApplicationSettings settings, SignInManager<ApplicationUser> signInManager)
    {
        _siteSetting = settings;
        _signInManager = signInManager;
    }

    public async Task<AccessTokenEntry> GenerateAsync(ApplicationUser user, Establishment? establishment,
        string? refreshToken)
    {
        
        var secretKey = Encoding.UTF8.GetBytes(_siteSetting.JwtSettings.SecretKey); // longer that 16 character
        var signingCredentials =
            new SigningCredentials(new SymmetricSecurityKey(secretKey), SecurityAlgorithms.HmacSha256Signature);

        var encryptionkey = Encoding.UTF8.GetBytes(_siteSetting.JwtSettings.EncryptKey); //must be 16 character
        var encryptingCredentials = new EncryptingCredentials(new SymmetricSecurityKey(encryptionkey),
            SecurityAlgorithms.Aes128KW, SecurityAlgorithms.Aes128CbcHmacSha256);

        var claims = await GetClaimsAsync(user, establishment);

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _siteSetting.JwtSettings.Issuer,
            Audience = _siteSetting.JwtSettings.Audience,
            IssuedAt = DateTime.UtcNow,
            NotBefore = DateTime.UtcNow.AddMinutes(_siteSetting.JwtSettings.NotBeforeMinutes),
            Expires = DateTime.UtcNow.AddMinutes(_siteSetting.JwtSettings.ExpirationMinutes),
            SigningCredentials = signingCredentials,
            EncryptingCredentials = encryptingCredentials,
            Subject = new ClaimsIdentity(claims),
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        var securityToken = tokenHandler.CreateJwtSecurityToken(descriptor);

        return new AccessTokenEntry(securityToken, establishment, refreshToken, establishment.Code);
    }

    private async Task<IEnumerable<Claim>> GetClaimsAsync(ApplicationUser user, Establishment? establishment)
    {
        var list = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Login),
            new Claim("EstablishmentId", establishment.Id.ToString()),
        };

        return list.ToArray();
    }

    public string? GenerateExternalToken()
    {
        try
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_siteSetting.JwtSettings.MonacoSecret));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim> { };

            var token = new JwtSecurityToken(
                claims: claims,
                signingCredentials: signingCredentials
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwt = tokenHandler.WriteToken(token);

            return jwt;
        }
        catch
        {
            return null;
        }
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var secretKey = Encoding.UTF8.GetBytes(_siteSetting.JwtSettings.SecretKey); // longer that 16 character
        var encryptionkey = Encoding.UTF8.GetBytes(_siteSetting.JwtSettings.EncryptKey); //must be 16 character

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(secretKey),
            ValidateIssuer = true,
            ValidIssuer = _siteSetting.JwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = _siteSetting.JwtSettings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            TokenDecryptionKey = new SymmetricSecurityKey(encryptionkey)
        };

        try
        {
            SecurityToken validatedToken;
            return tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
        }
        catch (Exception ex)
        {
            // Handle token validation errors
            return null;
        }
    }
}