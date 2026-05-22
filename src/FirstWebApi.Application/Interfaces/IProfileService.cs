using FirstWebApi.Application.DTOs.Response;

namespace FirstWebApi.Application.Interfaces;

public interface IProfileService
{
    Task<UserResponse> GetProfileAsync(Guid userId);
    Task<UserResponse> GetFullProfileAsync(Guid userId, string senha);
}
