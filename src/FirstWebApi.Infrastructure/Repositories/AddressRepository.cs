using FirstWebApi.Domain.Entities;
using FirstWebApi.Domain.Interfaces;
using FirstWebApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FirstWebApi.Infrastructure.Repositories;

public class AddressRepository(AppDbContext context) : IAddressRepository
{

    public async Task<Address?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await context.Addresses.AsNoTracking().FirstOrDefaultAsync(a => a.UserId == userId, cancellationToken);
    }

    public async Task AddAsync(Address address, CancellationToken cancellationToken = default)
    {
        await context.Addresses.AddAsync(address, cancellationToken);
    }
}
