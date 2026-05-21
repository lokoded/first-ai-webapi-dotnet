using FirstWebApi.Domain.Entities;

namespace FirstWebApi.Domain.Interfaces;

public interface IComicRepository
{
    Task<List<Comic>> GetByUserIdAsync(Guid userId);
    Task<Comic?> GetByIdAsync(Guid id);
    Task AddAsync(Comic comic);
    Task UpdateAsync(Comic comic);
    Task DeleteAsync(Comic comic);
}
