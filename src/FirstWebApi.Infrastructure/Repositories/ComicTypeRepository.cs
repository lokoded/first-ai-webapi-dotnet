using FirstWebApi.Domain.Entities;
using FirstWebApi.Domain.Interfaces;
using FirstWebApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FirstWebApi.Infrastructure.Repositories;

public class ComicTypeRepository(AppDbContext context) : IComicTypeRepository
{

    public async Task<List<ComicType>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.ComicTypes
            .OrderBy(ct => ct.Nome)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<ComicType?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.ComicTypes.FindAsync([id], cancellationToken);
    }

    public async Task AddAsync(ComicType comicType, CancellationToken cancellationToken = default)
    {
        await context.ComicTypes.AddAsync(comicType, cancellationToken);
    }

    public Task DeleteAsync(ComicType comicType, CancellationToken cancellationToken = default)
    {
        context.ComicTypes.Remove(comicType);
        return Task.CompletedTask;
    }

    public async Task<bool> HasComicsAsync(Guid comicTypeId, CancellationToken cancellationToken = default)
    {
        return await context.Comics.AnyAsync(c => c.ComicTypeId == comicTypeId, cancellationToken);
    }
}
