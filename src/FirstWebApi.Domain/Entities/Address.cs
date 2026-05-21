namespace FirstWebApi.Domain.Entities;

public class Address
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public byte[]? Ciphertext { get; private set; }
    public byte[]? Iv { get; private set; }
    public byte[]? Tag { get; private set; }
    public byte[]? EncryptedDataKey { get; private set; }

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }

    protected Address() { }

    public Address(Guid userId)
    {
        Id = Guid.NewGuid();
        UserId = userId;
    }

    public void SetEncryptedData(byte[] ciphertext, byte[] iv, byte[] tag, byte[] encryptedDataKey)
    {
        Ciphertext = ciphertext;
        Iv = iv;
        Tag = tag;
        EncryptedDataKey = encryptedDataKey;
        UpdatedAt = DateTime.UtcNow;
    }
}
