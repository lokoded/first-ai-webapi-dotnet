namespace FirstWebApi.Application.DTOs.Request;

public class ComicRequest
{
    public string Titulo { get; init; } = string.Empty;
    public string WebUrl { get; init; } = string.Empty;
    public Guid ComicTypeId { get; init; }
    public string? Observacao { get; init; }
}
