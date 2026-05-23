using FirstWebApi.Domain.Entities;

namespace FirstWebApi.Domain.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash);
    Task<List<RefreshToken>> GetActiveByUserIdAsync(Guid userId);
    /// <summary>
    /// Remove tokens expirados criados há mais de <paramref name="keepDays"/> dias.
    /// Tokens expirados recentes são preservados para auditoria.
    /// </summary>
    Task<int> DeleteExpiredAsync(int keepDays);
    Task AddAsync(RefreshToken refreshToken);
    Task UpdateAsync(RefreshToken refreshToken);
}
