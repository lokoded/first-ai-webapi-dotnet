using FirstWebApi.Application.DTOs.Request;
using FirstWebApi.Application.DTOs.Response;

namespace FirstWebApi.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken);
    Task RevokeRefreshTokensAsync(Guid userId);
    Task<UserResponse> GetProfileAsync(Guid userId);
    Task<UserResponse> GetFullProfileAsync(Guid userId, string senha);
}
