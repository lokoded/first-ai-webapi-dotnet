using FirstWebApi.Application.DTOs.Response;

namespace FirstWebApi.Application.Interfaces;

public interface IComicTypeService
{
    Task<List<ComicTypeResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ComicTypeResponse> CreateAsync(string name, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
