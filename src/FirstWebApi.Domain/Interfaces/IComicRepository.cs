using FirstWebApi.Domain.Entities;

namespace FirstWebApi.Domain.Interfaces;

public interface IComicRepository
{
    /// <summary>Retorna comics paginados de um usuário. A entidade <see cref="Comic"/> inclui <c>.Include(c => c.ComicType)</c>.</summary>
    Task<(List<Comic> Items, int TotalCount)> GetPaginatedByUserIdAsync(Guid userId, int page, int pageSize);

    /// <summary>Retorna comic por ID. A entidade <see cref="Comic"/> inclui <c>.Include(c => c.ComicType)</c>.</summary>
    Task<Comic?> GetByIdAsync(Guid id);

    Task AddAsync(Comic comic);
    Task DeleteAsync(Comic comic);
}
