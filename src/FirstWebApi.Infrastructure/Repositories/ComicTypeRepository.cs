using FirstWebApi.Domain.Entities;
using FirstWebApi.Domain.Interfaces;
using FirstWebApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FirstWebApi.Infrastructure.Repositories;

public class ComicTypeRepository(AppDbContext context) : IComicTypeRepository
{

    public async Task<List<ComicType>> GetAllAsync()
    {
        return await context.ComicTypes
            .OrderBy(ct => ct.Nome)
            .ToListAsync();
    }

    public async Task<ComicType?> GetByIdAsync(Guid id)
    {
        return await context.ComicTypes.FindAsync(id);
    }

    public async Task AddAsync(ComicType comicType)
    {
        await context.ComicTypes.AddAsync(comicType);
    }

    public Task DeleteAsync(ComicType comicType)
    {
        context.ComicTypes.Remove(comicType);
        return Task.CompletedTask;
    }

    public async Task<bool> HasComicsAsync(Guid comicTypeId)
    {
        return await context.Comics.AnyAsync(c => c.ComicTypeId == comicTypeId);
    }
}
