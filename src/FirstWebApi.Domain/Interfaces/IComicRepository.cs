using FirstWebApi.Domain.Entities;

namespace FirstWebApi.Domain.Interfaces;

public interface IComicRepository
{
    /// <summary>Retorna comics paginados de um usuário com dados do tipo de quadrinho.</summary>
    Task<(List<Comic> Items, int TotalCount)> GetPaginatedByUserIdAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>Retorna comic por ID com dados do tipo de quadrinho.</summary>
    Task<Comic?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(Comic comic, CancellationToken cancellationToken = default);
    Task DeleteAsync(Comic comic, CancellationToken cancellationToken = default);
}
