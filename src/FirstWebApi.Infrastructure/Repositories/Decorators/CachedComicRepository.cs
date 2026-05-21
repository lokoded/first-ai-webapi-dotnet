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
    private static readonly TimeSpan PaginatedTtl = TimeSpan.FromSeconds(30);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public CachedComicRepository(IComicRepository inner, IDistributedCache cache)
    {
        _inner = inner;
        _cache = cache;
    }

    public async Task<List<Comic>> GetByUserIdAsync(Guid userId)
        => await _inner.GetByUserIdAsync(userId);

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
        await _cache.RemoveAsync($"comics:user:{comic.UserId}:page:*");
    }

    public async Task UpdateAsync(Comic comic)
    {
        await _inner.UpdateAsync(comic);
        await _cache.RemoveAsync($"comic:{comic.Id}");
        await _cache.RemoveAsync($"comics:user:{comic.UserId}:page:*");
    }

    public async Task DeleteAsync(Comic comic)
    {
        await _inner.DeleteAsync(comic);
        await _cache.RemoveAsync($"comic:{comic.Id}");
        await _cache.RemoveAsync($"comics:user:{comic.UserId}:page:*");
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
