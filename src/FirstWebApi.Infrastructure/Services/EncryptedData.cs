namespace FirstWebApi.Infrastructure.Services;

internal readonly record struct EncryptedData(
    byte[] Ciphertext,
    byte[] Iv,
    byte[] Tag,
    byte[] EncryptedDataKey
);
