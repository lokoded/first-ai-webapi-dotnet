using FirstWebApi.Domain.Entities;
using FirstWebApi.Domain.Interfaces;
using FirstWebApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FirstWebApi.Infrastructure.Repositories;

public class ComicRepository : IComicRepository
{
    private readonly AppDbContext _context;

    public ComicRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Comic>> GetByUserIdAsync(Guid userId)
    {
        return await _context.Comics
            .Include(c => c.ComicType)
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<Comic?> GetByIdAsync(Guid id)
    {
        return await _context.Comics
            .Include(c => c.ComicType)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task AddAsync(Comic comic)
    {
        await _context.Comics.AddAsync(comic);
    }

    public Task UpdateAsync(Comic comic)
    {
        _context.Comics.Update(comic);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Comic comic)
    {
        _context.Comics.Remove(comic);
        return Task.CompletedTask;
    }
}
