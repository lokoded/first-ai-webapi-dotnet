using FirstWebApi.Application.DTOs;
using FirstWebApi.Domain.Entities;

namespace FirstWebApi.Application.Interfaces;

public interface ISensitiveDataService
{
    Task EncryptCpfAsync(User user, string? cpf);
    Task EncryptRgAsync(User user, string? rg);
    Task<Address?> EncryptEnderecoAsync(Guid userId, EnderecoInfo? endereco);
    Task<(string? cpf, string? rg, EnderecoInfo? endereco)> DecryptUserDataAsync(User user, Guid userId);
}
