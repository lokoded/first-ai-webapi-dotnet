using System.Text.Json;
using FirstWebApi.Domain.Entities;
using FirstWebApi.Domain.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace FirstWebApi.Infrastructure.Repositories.Decorators;

public class CachedComicRepository(IComicRepository inner, IDistributedCache cache, IConnectionMultiplexer redis) : IComicRepository
{
    private readonly IComicRepository _inner = inner;
    private readonly IDistributedCache _cache = cache;
    private readonly IConnectionMultiplexer _redis = redis;

    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan PaginatedTtl = TimeSpan.FromSeconds(30);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task<(List<Comic> Items, int TotalCount)> GetPaginatedByUserIdAsync(Guid userId, int page, int pageSize)
    {
        var key = $"comics:user:{userId}:page:{page}:size:{pageSize}";
        var cached = await _cache.GetStringAsync(key);
        if (cached is not null)
            return JsonSerializer.Deserialize<CachedPaginatedResult>(cached)!.ToTuple();

        var result = await _inner.GetPaginatedByUserIdAsync(userId, page, pageSize);
        await _cache.SetStringAsync(key, JsonSerializer.Serialize(new CachedPaginatedResult(result), JsonOptions), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = PaginatedTtl
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
        await InvalidatePaginatedCacheAsync(comic.UserId);
    }

    public async Task UpdateAsync(Comic comic)
    {
        await _inner.UpdateAsync(comic);
        await _cache.RemoveAsync($"comic:{comic.Id}");
        await InvalidatePaginatedCacheAsync(comic.UserId);
    }

    public async Task DeleteAsync(Comic comic)
    {
        await _inner.DeleteAsync(comic);
        await _cache.RemoveAsync($"comic:{comic.Id}");
        await InvalidatePaginatedCacheAsync(comic.UserId);
    }

    private async Task InvalidatePaginatedCacheAsync(Guid userId)
    {
        var server = _redis.GetServer(_redis.GetEndPoints()[0]);
        var keys = server.KeysAsync(pattern: $"comics:user:{userId}:page:*");
        var db = _redis.GetDatabase();
        var keyArray = await keys.ToArrayAsync();
        if (keyArray.Length > 0)
            await db.KeyDeleteAsync(keyArray);
    }

    private sealed class CachedPaginatedResult
    {
        public List<Comic> Items { get; set; } = [];
        public int TotalCount { get; set; }

        public CachedPaginatedResult() { }

        public CachedPaginatedResult((List<Comic> Items, int TotalCount) tuple)
        {
            Items = tuple.Items;
            TotalCount = tuple.TotalCount;
        }

        public (List<Comic> Items, int TotalCount) ToTuple() => (Items, TotalCount);
    }
}
