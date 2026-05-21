using System.Security.Claims;

namespace FirstWebApi.Application.Interfaces;

public interface ITokenService
{
    string GenerateToken(Guid userId, string email, string nome, IList<string> roles);
    ClaimsPrincipal? ValidateToken(string token);
    (string refreshToken, string tokenHash) GenerateRefreshToken();
    string HashToken(string token);
}
