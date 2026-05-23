using FirstWebApi.Domain.Entities;

namespace FirstWebApi.Domain.Interfaces;

public interface IComicTypeRepository
{
    Task<List<ComicType>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ComicType?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(ComicType comicType, CancellationToken cancellationToken = default);
    Task DeleteAsync(ComicType comicType, CancellationToken cancellationToken = default);
    Task<bool> HasComicsAsync(Guid comicTypeId, CancellationToken cancellationToken = default);
}
