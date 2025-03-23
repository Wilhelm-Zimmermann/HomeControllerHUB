using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Shared.Common.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace HomeControllerHUB.Domain.Models;

public class AccessTokenEntry
{
    [BindProperty(Name = OAuth2Constants.AccessToken)]
    [JsonPropertyName(OAuth2Constants.AccessToken)]
    public string AccessToken { get; set; }

    [BindProperty(Name = OAuth2Constants.RefreshToken)]
    [JsonPropertyName(OAuth2Constants.RefreshToken)]
    public string RefreshToken { get; set; }

    [BindProperty(Name = OAuth2Constants.TokenType)]
    [JsonPropertyName(OAuth2Constants.TokenType)]
    public string TokenType { get; set; }

    [BindProperty(Name = OAuth2Constants.ExpiresIn)]
    [JsonPropertyName(OAuth2Constants.ExpiresIn)]
    public int ExpiresIn { get; set; }
    public string? EstablishmentTenantSequential { get; set; }

    public AccessTokenEntry(JwtSecurityToken securityToken, Establishment? establishment, string? refreshToken, string? establishmentTenantSequential) : base()
    {
        AccessToken = new JwtSecurityTokenHandler().WriteToken(securityToken);
        TokenType = SecurityNames.Bearer;
        ExpiresIn = (int)(securityToken.ValidTo - DateTimeOffset.UtcNow).TotalSeconds;
        RefreshToken = refreshToken != null ? refreshToken : GenerateRefreshToken(establishment);
        EstablishmentTenantSequential = establishmentTenantSequential;
    }

    private static string GenerateRefreshToken(Establishment? establishment)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);


        var refreshTokenDescriptor = new SecurityTokenDescriptor
        {
            // Adicione a claim personalizada ao token de atualização
            Subject = new ClaimsIdentity(new[]
            {
                // Claim personalizada
                new Claim("EstablishmentId", establishment != null && establishment.Id != null ? establishment.Id.ToString() : ""),
                new Claim("TokenNumber", Convert.ToBase64String(randomNumber))
            })
        };

        var refreshToken = tokenHandler.CreateToken(refreshTokenDescriptor);
        return tokenHandler.WriteToken(refreshToken);
    }
}