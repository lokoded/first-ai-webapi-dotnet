using FirstWebApi.Domain.Entities;
using FirstWebApi.Domain.Interfaces;
using FirstWebApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FirstWebApi.Infrastructure.Repositories;

public class ComicRepository(AppDbContext context) : IComicRepository
{

    public async Task<List<Comic>> GetByUserIdAsync(Guid userId)
    {
        return await context.Comics
            .Include(c => c.ComicType)
            .Where(c => c.UserId == userId)
            .ToListAsync();
    }

    public async Task<(List<Comic> Items, int TotalCount)> GetPaginatedByUserIdAsync(Guid userId, int page, int pageSize)
    {
        var query = context.Comics
            .Include(c => c.ComicType)
            .Where(c => c.UserId == userId);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Comic?> GetByIdAsync(Guid id)
    {
        return await context.Comics
            .Include(c => c.ComicType)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task AddAsync(Comic comic)
    {
        await context.Comics.AddAsync(comic);
    }

    public Task UpdateAsync(Comic comic)
    {
        context.Comics.Update(comic);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Comic comic)
    {
        context.Comics.Remove(comic);
        return Task.CompletedTask;
    }
}
