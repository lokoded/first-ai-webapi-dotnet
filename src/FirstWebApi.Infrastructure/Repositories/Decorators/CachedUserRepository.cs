using System.Text.Json;
using FirstWebApi.Domain.Entities;
using FirstWebApi.Domain.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace FirstWebApi.Infrastructure.Repositories.Decorators;

public class CachedUserRepository : IUserRepository
{
    private readonly IUserRepository _inner;
    private readonly IDistributedCache _cache;
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(5);

    public CachedUserRepository(IUserRepository inner, IDistributedCache cache)
    {
        _inner = inner;
        _cache = cache;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        var key = $"user:{id}";
        var cached = await _cache.GetStringAsync(key);
        if (cached is not null)
            return JsonSerializer.Deserialize<User>(cached);

        var result = await _inner.GetByIdAsync(id);
        if (result is not null)
            await _cache.SetStringAsync(key, JsonSerializer.Serialize(result), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = Ttl
            });
        return result;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _inner.GetByEmailAsync(email);
    }

    public async Task AddAsync(User user)
    {
        await _inner.AddAsync(user);
    }

    public async Task UpdateAsync(User user)
    {
        await _inner.UpdateAsync(user);
        await _cache.RemoveAsync($"user:{user.Id}");
    }
}
