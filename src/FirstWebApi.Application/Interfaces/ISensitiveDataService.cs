using FirstWebApi.Application.DTOs;
using FirstWebApi.Domain.Entities;

namespace FirstWebApi.Application.Interfaces;

public interface ISensitiveDataService
{
    Task EncryptCpfAsync(User user, string? cpf, CancellationToken cancellationToken = default);
    Task EncryptRgAsync(User user, string? rg, CancellationToken cancellationToken = default);
    Task<Address?> EncryptEnderecoAsync(Guid userId, EnderecoInfo? endereco, CancellationToken cancellationToken = default);
    Task<(string? cpf, string? rg, EnderecoInfo? endereco)> DecryptUserDataAsync(User user, Guid userId, CancellationToken cancellationToken = default);
}
