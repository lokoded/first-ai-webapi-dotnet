namespace FirstWebApi.Application.DTOs.Request;

public class ComicRequest
{
    public string Titulo { get; set; } = string.Empty;
    public string WebUrl { get; set; } = string.Empty;
    public Guid ComicTypeId { get; set; }
    public string? Observacao { get; set; }
}
