using FirstWebApi.Domain.ValueObjects;

namespace FirstWebApi.Domain.Entities;

public class Address
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public byte[]? Dados { get; private set; }

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }

    public DadoProtegido? Data => Dados is not null
        ? new DadoProtegido(Dados)
        : null;

    protected Address() { }

    public Address(Guid userId)
    {
        Id = Guid.NewGuid();
        UserId = userId;
    }

    public void SetEncryptedData(DadoProtegido data)
        => Dados = data.Valor;
}
