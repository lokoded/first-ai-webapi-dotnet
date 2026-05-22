using FirstWebApi.Domain.Entities;
using FirstWebApi.Domain.Interfaces;
using FirstWebApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FirstWebApi.Infrastructure.Repositories;

public class AddressRepository(AppDbContext context) : IAddressRepository
{

    public async Task<Address?> GetByUserIdAsync(Guid userId)
    {
        return await context.Addresses.FirstOrDefaultAsync(a => a.UserId == userId);
    }

    public async Task AddAsync(Address address)
    {
        await context.Addresses.AddAsync(address);
    }
}
