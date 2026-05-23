using FirstWebApi.Domain.ValueObjects;

namespace FirstWebApi.Application.Interfaces;

public interface IEncryptionService
{
    Task<DadoProtegido> EncryptAsync(string plaintext, CancellationToken cancellationToken = default);
    Task<string> DecryptAsync(DadoProtegido data, CancellationToken cancellationToken = default);
}
