using FirstWebApi.Application.DTOs.Request;
using FirstWebApi.Application.DTOs.Response;

namespace FirstWebApi.Application.Interfaces;

public interface IComicService
{
    Task<PaginatedResult<ComicResponse>> GetAllAsync(Guid userId, int page = 1, int pageSize = 20);
    Task<ComicResponse?> GetByIdAsync(Guid id, Guid userId);
    Task<ComicResponse> CreateAsync(ComicRequest request, Guid userId);
    Task<ComicResponse?> UpdateAsync(Guid id, ComicRequest request, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid userId);
}
