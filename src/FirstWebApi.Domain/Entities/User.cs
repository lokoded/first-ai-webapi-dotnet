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

    protected User() { }

    public User(string nome, string userName, string email) : base()
    {
        Id = Guid.NewGuid();
        Nome = nome;
        UserName = userName;
        Email = email;
    }

    public void SetRoleFromIdentity() => Role = EUserRole.User;

    public void SetCpfData(byte[] ciphertext, byte[] iv, byte[] tag, byte[] encryptedDataKey)
    {
        CpfCiphertext = ciphertext;
        CpfIv = iv;
        CpfTag = tag;
        CpfEncryptedDataKey = encryptedDataKey;
    }

    public void SetRgData(byte[] ciphertext, byte[] iv, byte[] tag, byte[] encryptedDataKey)
    {
        RgCiphertext = ciphertext;
        RgIv = iv;
        RgTag = tag;
        RgEncryptedDataKey = encryptedDataKey;
    }
}
