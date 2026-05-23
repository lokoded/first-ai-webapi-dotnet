using FirstWebApi.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;

namespace FirstWebApi.Domain.Entities;

public class User : IdentityUser<Guid>
{
    public string Nome { get; private set; } = string.Empty;

    public byte[]? CpfDados { get; private set; }
    public byte[]? RgDados { get; private set; }

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }

    public DadoProtegido? CpfData => CpfDados is not null
        ? new DadoProtegido(CpfDados)
        : null;

    public DadoProtegido? RgData => RgDados is not null
        ? new DadoProtegido(RgDados)
        : null;

    protected User() { }

    public User(string nome, string userName, string email) : base()
    {
        Id = Guid.NewGuid();
        Nome = nome;
        UserName = userName;
        Email = email;
    }

    public void SetCpfData(DadoProtegido data)
        => CpfDados = data.Valor;

    public void SetRgData(DadoProtegido data)
        => RgDados = data.Valor;
}
