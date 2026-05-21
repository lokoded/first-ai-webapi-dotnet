namespace FirstWebApi.Application.Interfaces;

public interface IEncryptionService
{
    Task<(byte[] ciphertext, byte[] iv, byte[] tag, byte[] encryptedDataKey)> EncryptAsync(string plaintext);
    Task<string> DecryptAsync(byte[] ciphertext, byte[] iv, byte[] tag, byte[] encryptedDataKey);
}
