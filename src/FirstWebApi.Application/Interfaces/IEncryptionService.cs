using FirstWebApi.Domain.ValueObjects;

namespace FirstWebApi.Application.Interfaces;

public interface IEncryptionService
{
    Task<EncryptedData> EncryptAsync(string plaintext);
    Task<string> DecryptAsync(EncryptedData data);
}
