using FirstWebApi.Domain.Entities;
using FirstWebApi.Domain.Interfaces;
using FirstWebApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FirstWebApi.Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _context;

    public RefreshTokenRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash)
    {
        return await _context.RefreshTokens
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash);
    }

    public async Task<List<RefreshToken>> GetActiveByUserIdAsync(Guid userId)
    {
        return await _context.RefreshTokens
            .Where(x => x.UserId == userId && !x.RevokedAt.HasValue && x.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();
    }

    public async Task<int> DeleteExpiredAsync(int keepDays)
    {
        var cutoff = DateTime.UtcNow.AddDays(-keepDays);
        return await _context.RefreshTokens
            .Where(t => t.ExpiresAt < DateTime.UtcNow && t.CreatedAt < cutoff)
            .ExecuteDeleteAsync();
    }

    public async Task AddAsync(RefreshToken refreshToken)
    {
        await _context.RefreshTokens.AddAsync(refreshToken);
    }

    public void Update(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Update(refreshToken);
    }
}
