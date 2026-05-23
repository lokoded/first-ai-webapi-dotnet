namespace FirstWebApi.Domain.ValueObjects;

public readonly record struct EncryptedData(
    byte[] Ciphertext,
    byte[] Iv,
    byte[] Tag,
    byte[] EncryptedDataKey
);
