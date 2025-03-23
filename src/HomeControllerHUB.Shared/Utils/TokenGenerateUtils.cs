using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace HomeControllerHUB.Shared.Utils;

public static class TokenGenerateUtils
{
    private static readonly string SecretKey = "kEeQXprCS3Jubzt8FY6jB4sAaVhDnK7mLUNRf9yvqdgwPTG5ZH";
    private static readonly string EncryptKey = "qB63uaSpAy4FT8Gs";
    private static readonly string Issuer = "HomeControllerHUB";
    private static readonly string Audience = "HomeControllerHUB";

    public static string Generate()
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()));
    }

    public static string GenerateConfirmationCode()
    {
        var rdn = new Random();
        var code = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"[rdn.Next(0, 25)].ToString();

        for (int i = 0; i < 5; i++) code += rdn.Next(0, 9).ToString();

        return code;
    }

    public static string GenerateJwtToken(Claim[] claims, DateTime? expires = null, DateTime? notBefore = null)
    {
        var secretKey = Encoding.UTF8.GetBytes(SecretKey);
        var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKey), SecurityAlgorithms.HmacSha256Signature);

        var encryptionkey = Encoding.UTF8.GetBytes(EncryptKey); //must be 16 character
        var encryptingCredentials = new EncryptingCredentials(new SymmetricSecurityKey(encryptionkey), SecurityAlgorithms.Aes128KW, SecurityAlgorithms.Aes128CbcHmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = Issuer,
            Audience = Audience,
            IssuedAt = DateTime.UtcNow,
            SigningCredentials = signingCredentials,
            EncryptingCredentials = encryptingCredentials,
            Subject = new ClaimsIdentity(claims)
        };
        if (notBefore != null)
        {
            tokenDescriptor.NotBefore = notBefore;
        }
        if (expires != null)
        {
            tokenDescriptor.Expires = expires;
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.CreateJwtSecurityToken(tokenDescriptor);
        return tokenHandler.WriteToken(securityToken);
    }

    public static TokenValidationParameters tokenValidationParameters
    {
        get
        {
            var key = Encoding.UTF8.GetBytes(SecretKey);
            var encryptkey = Encoding.UTF8.GetBytes(EncryptKey);

            return new TokenValidationParameters
            {
                ClockSkew = TimeSpan.Zero, // default: 5 min
                RequireSignedTokens = true,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),

                RequireExpirationTime = true,
                ValidateLifetime = true,

                ValidateAudience = true, //default : false
                ValidAudience = Audience,

                ValidateIssuer = true, //default : false
                ValidIssuer = Issuer,

                TokenDecryptionKey = new SymmetricSecurityKey(encryptkey)
            };
        }
    }

    public static TokenValidationParameters tokenValidationParametersWithoutValidateLifetime
    {
        get
        {
            var tokenValidationParametersLocal = tokenValidationParameters;
            tokenValidationParametersLocal.ValidateLifetime = false;
            return tokenValidationParametersLocal;
        }
    }

    public static bool ValidateJwtToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            tokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            // Token inválido
            return false;
        }
    }

    public static void AddJwtAuthentication(this IServiceCollection services, bool validateLifetime = true)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = tokenValidationParameters;
            if (!validateLifetime)
                options.TokenValidationParameters = tokenValidationParametersWithoutValidateLifetime;
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    return Task.CompletedTask;
                },
                OnChallenge = context =>
                {
                    return Task.CompletedTask;
                }
            };
        });
    }

}