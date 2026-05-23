using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FirstWebApi.Application.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FirstWebApi.Infrastructure.Services;

public class JwtSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
}

public class TokenService(IOptions<JwtSettings> jwtSettings) : ITokenService
{

    public string GenerateToken(Guid userId, string email, string nome, IList<string> roles)
    {
        var secretKey = jwtSettings.Value.SecretKey;
        if (string.IsNullOrEmpty(secretKey) || secretKey.Length < 32)
            throw new InvalidOperationException("JWT SecretKey deve ter no mínimo 32 caracteres (256 bits).");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Name, nome),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var token = new JwtSecurityToken(
            issuer: jwtSettings.Value.Issuer,
            audience: jwtSettings.Value.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        var secretKey = jwtSettings.Value.SecretKey;
        if (string.IsNullOrEmpty(secretKey) || secretKey.Length < 32)
            throw new InvalidOperationException("JWT SecretKey deve ter no mínimo 32 caracteres (256 bits).");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var result = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Value.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtSettings.Value.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);

            return result;
        }
        catch
        {
            return null;
        }
    }

    public (string refreshToken, string tokenHash) GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        var refreshToken = Convert.ToBase64String(randomBytes);
        var tokenHash = HashToken(refreshToken);
        return (refreshToken, tokenHash);
    }

    public string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }
}
