namespace FirstWebApi.Application.DTOs.Response;

public class ComicResponse
{
    public Guid Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string WebUrl { get; set; } = string.Empty;
    public string? Observacao { get; set; }
    public Guid ComicTypeId { get; set; }
    public string ComicTypeNome { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
