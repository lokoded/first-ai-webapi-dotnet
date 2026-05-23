using FirstWebApi.Domain.ValueObjects;

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

    public EncryptedData? Data => Ciphertext is not null
        ? new EncryptedData(Ciphertext, Iv!, Tag!, EncryptedDataKey!)
        : null;

    protected Address() { }

    public Address(Guid userId)
    {
        Id = Guid.NewGuid();
        UserId = userId;
    }

    public void SetEncryptedData(EncryptedData data)
    {
        Ciphertext = data.Ciphertext;
        Iv = data.Iv;
        Tag = data.Tag;
        EncryptedDataKey = data.EncryptedDataKey;
    }
}
