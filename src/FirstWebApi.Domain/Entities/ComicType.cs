namespace FirstWebApi.Domain.Entities;

public class ComicType
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }

    protected ComicType() { }

    public ComicType(string nome)
    {
        Id = Guid.NewGuid();
        Nome = nome;
    }

    public void Update(string nome)
    {
        Nome = nome;
        UpdatedAt = DateTime.UtcNow;
    }
}
