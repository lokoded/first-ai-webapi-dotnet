using FirstWebApi.Domain.Entities;

namespace FirstWebApi.Domain.Interfaces;

public interface IComicTypeRepository
{
    Task<List<ComicType>> GetAllAsync();
    Task<ComicType?> GetByIdAsync(Guid id);
    Task AddAsync(ComicType comicType);
    Task DeleteAsync(ComicType comicType);
    Task<bool> HasComicsAsync(Guid comicTypeId);
}
