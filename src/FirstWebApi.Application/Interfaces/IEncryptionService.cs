using FirstWebApi.Domain.ValueObjects;

namespace FirstWebApi.Application.Interfaces;

public interface IEncryptionService
{
    Task<DadoProtegido> EncryptAsync(string plaintext);
    Task<string> DecryptAsync(DadoProtegido data);
}
