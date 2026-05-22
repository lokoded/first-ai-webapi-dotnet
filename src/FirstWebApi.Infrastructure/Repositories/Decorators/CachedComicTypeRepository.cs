using System.Text.Json;
using FirstWebApi.Domain.Entities;
using FirstWebApi.Domain.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace FirstWebApi.Infrastructure.Repositories.Decorators;

public class CachedComicTypeRepository(IComicTypeRepository inner, IDistributedCache cache) : IComicTypeRepository
{
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(30);

    public async Task<List<ComicType>> GetAllAsync()
    {
        const string key = "comic-types:all";
        var cached = await cache.GetStringAsync(key);
        if (cached is not null)
            return JsonSerializer.Deserialize<List<ComicType>>(cached)!;

        var result = await inner.GetAllAsync();
        await cache.SetStringAsync(key, JsonSerializer.Serialize(result), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = Ttl
        });
        return result;
    }

    public async Task<ComicType?> GetByIdAsync(Guid id)
    {
        var key = $"comic-type:{id}";
        var cached = await cache.GetStringAsync(key);
        if (cached is not null)
            return JsonSerializer.Deserialize<ComicType>(cached);

        var result = await inner.GetByIdAsync(id);
        if (result is not null)
            await cache.SetStringAsync(key, JsonSerializer.Serialize(result), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = Ttl
            });
        return result;
    }

    public async Task AddAsync(ComicType comicType)
    {
        await inner.AddAsync(comicType);
        await cache.RemoveAsync("comic-types:all");
    }

    public async Task DeleteAsync(ComicType comicType)
    {
        await inner.DeleteAsync(comicType);
        await cache.RemoveAsync("comic-types:all");
        await cache.RemoveAsync($"comic-type:{comicType.Id}");
    }

    public async Task<bool> HasComicsAsync(Guid comicTypeId)
    {
        return await inner.HasComicsAsync(comicTypeId);
    }
}
