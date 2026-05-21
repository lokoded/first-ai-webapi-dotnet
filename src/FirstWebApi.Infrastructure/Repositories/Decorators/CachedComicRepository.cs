using System.Text.Json;
using FirstWebApi.Domain.Entities;
using FirstWebApi.Domain.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace FirstWebApi.Infrastructure.Repositories.Decorators;

public class CachedComicRepository : IComicRepository
{
    private readonly IComicRepository _inner;
    private readonly IDistributedCache _cache;
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(5);

    public CachedComicRepository(IComicRepository inner, IDistributedCache cache)
    {
        _inner = inner;
        _cache = cache;
    }

    public async Task<List<Comic>> GetByUserIdAsync(Guid userId)
    {
        var key = $"comics:user:{userId}";
        var cached = await _cache.GetStringAsync(key);
        if (cached is not null)
            return JsonSerializer.Deserialize<List<Comic>>(cached)!;

        var result = await _inner.GetByUserIdAsync(userId);
        await _cache.SetStringAsync(key, JsonSerializer.Serialize(result), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = Ttl
        });
        return result;
    }

    public async Task<Comic?> GetByIdAsync(Guid id)
    {
        var key = $"comic:{id}";
        var cached = await _cache.GetStringAsync(key);
        if (cached is not null)
            return JsonSerializer.Deserialize<Comic>(cached);

        var result = await _inner.GetByIdAsync(id);
        if (result is not null)
            await _cache.SetStringAsync(key, JsonSerializer.Serialize(result), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = Ttl
            });
        return result;
    }

    public async Task AddAsync(Comic comic)
    {
        await _inner.AddAsync(comic);
        await _cache.RemoveAsync($"comics:user:{comic.UserId}");
    }

    public async Task UpdateAsync(Comic comic)
    {
        await _inner.UpdateAsync(comic);
        await _cache.RemoveAsync($"comics:user:{comic.UserId}");
        await _cache.RemoveAsync($"comic:{comic.Id}");
    }

    public async Task DeleteAsync(Comic comic)
    {
        await _inner.DeleteAsync(comic);
        await _cache.RemoveAsync($"comics:user:{comic.UserId}");
        await _cache.RemoveAsync($"comic:{comic.Id}");
    }
}
