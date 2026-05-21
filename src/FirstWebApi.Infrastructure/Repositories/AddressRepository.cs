using FirstWebApi.Domain.Entities;
using FirstWebApi.Domain.Interfaces;
using FirstWebApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FirstWebApi.Infrastructure.Repositories;

public class AddressRepository : IAddressRepository
{
    private readonly AppDbContext _context;

    public AddressRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Address?> GetByUserIdAsync(Guid userId)
    {
        return await _context.Addresses.FirstOrDefaultAsync(a => a.UserId == userId);
    }

    public async Task AddAsync(Address address)
    {
        await _context.Addresses.AddAsync(address);
    }
}
