using FirstWebApi.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;

namespace FirstWebApi.Domain.Entities;

public class User : IdentityUser<Guid>
{
    public string Nome { get; private set; } = string.Empty;

    public byte[]? CpfCiphertext { get; private set; }
    public byte[]? CpfIv { get; private set; }
    public byte[]? CpfTag { get; private set; }
    public byte[]? CpfEncryptedDataKey { get; private set; }

    public byte[]? RgCiphertext { get; private set; }
    public byte[]? RgIv { get; private set; }
    public byte[]? RgTag { get; private set; }
    public byte[]? RgEncryptedDataKey { get; private set; }

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }

    public EncryptedData? CpfData => CpfCiphertext is not null
        ? new EncryptedData(CpfCiphertext, CpfIv!, CpfTag!, CpfEncryptedDataKey!)
        : null;

    public EncryptedData? RgData => RgCiphertext is not null
        ? new EncryptedData(RgCiphertext, RgIv!, RgTag!, RgEncryptedDataKey!)
        : null;

    protected User() { }

    public User(string nome, string userName, string email) : base()
    {
        Id = Guid.NewGuid();
        Nome = nome;
        UserName = userName;
        Email = email;
    }

    public void SetCpfData(EncryptedData data)
    {
        CpfCiphertext = data.Ciphertext;
        CpfIv = data.Iv;
        CpfTag = data.Tag;
        CpfEncryptedDataKey = data.EncryptedDataKey;
    }

    public void SetRgData(EncryptedData data)
    {
        RgCiphertext = data.Ciphertext;
        RgIv = data.Iv;
        RgTag = data.Tag;
        RgEncryptedDataKey = data.EncryptedDataKey;
    }
}
