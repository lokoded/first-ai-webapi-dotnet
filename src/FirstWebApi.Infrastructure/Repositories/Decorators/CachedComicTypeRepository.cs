using System.Text.Json;
using FirstWebApi.Domain.Entities;
using FirstWebApi.Domain.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace FirstWebApi.Infrastructure.Repositories.Decorators;

public class CachedComicTypeRepository : IComicTypeRepository
{
    private readonly IComicTypeRepository _inner;
    private readonly IDistributedCache _cache;
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(30);

    public CachedComicTypeRepository(IComicTypeRepository inner, IDistributedCache cache)
    {
        _inner = inner;
        _cache = cache;
    }

    public async Task<List<ComicType>> GetAllAsync()
    {
        const string key = "comic-types:all";
        var cached = await _cache.GetStringAsync(key);
        if (cached is not null)
            return JsonSerializer.Deserialize<List<ComicType>>(cached)!;

        var result = await _inner.GetAllAsync();
        await _cache.SetStringAsync(key, JsonSerializer.Serialize(result), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = Ttl
        });
        return result;
    }

    public async Task<ComicType?> GetByIdAsync(Guid id)
    {
        var key = $"comic-type:{id}";
        var cached = await _cache.GetStringAsync(key);
        if (cached is not null)
            return JsonSerializer.Deserialize<ComicType>(cached);

        var result = await _inner.GetByIdAsync(id);
        if (result is not null)
            await _cache.SetStringAsync(key, JsonSerializer.Serialize(result), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = Ttl
            });
        return result;
    }

    public async Task AddAsync(ComicType comicType)
    {
        await _inner.AddAsync(comicType);
        await _cache.RemoveAsync("comic-types:all");
    }

    public async Task DeleteAsync(ComicType comicType)
    {
        await _inner.DeleteAsync(comicType);
        await _cache.RemoveAsync("comic-types:all");
        await _cache.RemoveAsync($"comic-type:{comicType.Id}");
    }

    public async Task<bool> HasComicsAsync(Guid comicTypeId)
    {
        return await _inner.HasComicsAsync(comicTypeId);
    }
}
