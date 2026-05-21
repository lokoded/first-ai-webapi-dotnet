using FirstWebApi.Domain.Entities;

namespace FirstWebApi.Domain.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash);
    Task<List<RefreshToken>> GetActiveByUserIdAsync(Guid userId);
    Task AddAsync(RefreshToken refreshToken);
    void Update(RefreshToken refreshToken);
}
