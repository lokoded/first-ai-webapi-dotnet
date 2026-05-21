using FirstWebApi.Application.DTOs.Response;

namespace FirstWebApi.Application.Interfaces;

public interface IComicTypeService
{
    Task<List<ComicTypeResponse>> GetAllAsync();
    Task<ComicTypeResponse> CreateAsync(string nome);
    Task<bool> DeleteAsync(Guid id);
}
