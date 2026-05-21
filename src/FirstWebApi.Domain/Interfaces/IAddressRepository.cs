using FirstWebApi.Domain.Entities;

namespace FirstWebApi.Domain.Interfaces;

public interface IAddressRepository
{
    Task<Address?> GetByUserIdAsync(Guid userId);
    Task AddAsync(Address address);
}
