using FirstWebApi.Application.DTOs.Response;

namespace FirstWebApi.Application.Interfaces;

public interface IProfileService
{
    Task<UserResponse> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserResponse> GetFullProfileAsync(Guid userId, string senha, CancellationToken cancellationToken = default);
}
