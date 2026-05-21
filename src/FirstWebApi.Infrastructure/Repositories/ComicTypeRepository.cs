using FirstWebApi.Domain.Entities;
using FirstWebApi.Domain.Interfaces;
using FirstWebApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FirstWebApi.Infrastructure.Repositories;

public class ComicTypeRepository : IComicTypeRepository
{
    private readonly AppDbContext _context;

    public ComicTypeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ComicType>> GetAllAsync()
    {
        return await _context.ComicTypes
            .OrderBy(ct => ct.Nome)
            .ToListAsync();
    }

    public async Task<ComicType?> GetByIdAsync(Guid id)
    {
        return await _context.ComicTypes.FindAsync(id);
    }

    public async Task AddAsync(ComicType comicType)
    {
        await _context.ComicTypes.AddAsync(comicType);
    }

    public Task DeleteAsync(ComicType comicType)
    {
        _context.ComicTypes.Remove(comicType);
        return Task.CompletedTask;
    }

    public async Task<bool> HasComicsAsync(Guid comicTypeId)
    {
        return await _context.Comics.AnyAsync(c => c.ComicTypeId == comicTypeId);
    }
}
