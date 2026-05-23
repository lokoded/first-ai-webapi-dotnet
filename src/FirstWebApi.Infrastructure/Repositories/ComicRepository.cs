using FirstWebApi.Domain.Entities;
using FirstWebApi.Domain.Interfaces;
using FirstWebApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FirstWebApi.Infrastructure.Repositories;

public class ComicRepository(AppDbContext context) : IComicRepository
{

    public async Task<(List<Comic> Items, int TotalCount)> GetPaginatedByUserIdAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = context.Comics
            .Include(c => c.ComicType)
            .Where(c => c.UserId == userId);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<Comic?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Comics
            .Include(c => c.ComicType)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task AddAsync(Comic comic, CancellationToken cancellationToken = default)
    {
        await context.Comics.AddAsync(comic, cancellationToken);
    }

    public Task DeleteAsync(Comic comic, CancellationToken cancellationToken = default)
    {
        context.Comics.Remove(comic);
        return Task.CompletedTask;
    }
}
