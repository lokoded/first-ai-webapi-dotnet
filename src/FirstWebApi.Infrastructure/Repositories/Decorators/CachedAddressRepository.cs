using System.Text.Json;
using FirstWebApi.Domain.Entities;
using FirstWebApi.Domain.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace FirstWebApi.Infrastructure.Repositories.Decorators;

public class CachedAddressRepository : IAddressRepository
{
    private readonly IAddressRepository _inner;
    private readonly IDistributedCache _cache;
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(5);

    public CachedAddressRepository(IAddressRepository inner, IDistributedCache cache)
    {
        _inner = inner;
        _cache = cache;
    }

    public async Task<Address?> GetByUserIdAsync(Guid userId)
    {
        // Não cacheia Address pois contém dados sensíveis cifrados (endereço)
        return await _inner.GetByUserIdAsync(userId);
    }

    public async Task AddAsync(Address address)
    {
        await _inner.AddAsync(address);
        await _cache.RemoveAsync($"address:user:{address.UserId}");
    }
}
