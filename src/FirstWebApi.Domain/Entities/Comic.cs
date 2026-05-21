namespace FirstWebApi.Domain.Entities;

public class Comic
{
    public Guid Id { get; private set; }
    public string Titulo { get; private set; } = string.Empty;
    public string WebUrl { get; private set; } = string.Empty;
    public string? Observacao { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }

    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public Guid ComicTypeId { get; private set; }
    public ComicType ComicType { get; private set; } = null!;

    protected Comic() { }

    public Comic(string titulo, string webUrl, Guid userId, Guid comicTypeId, string? observacao = null)
    {
        Id = Guid.NewGuid();
        Titulo = titulo;
        WebUrl = webUrl;
        Observacao = observacao;
        UserId = userId;
        ComicTypeId = comicTypeId;
    }

    public void Update(string titulo, string webUrl, Guid comicTypeId, string? observacao)
    {
        Titulo = titulo;
        WebUrl = webUrl;
        ComicTypeId = comicTypeId;
        Observacao = observacao;
        UpdatedAt = DateTime.UtcNow;
    }
}
