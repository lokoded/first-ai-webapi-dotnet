using System.Text.Json.Serialization;

namespace FirstWebApi.Application.DTOs;

public class EnderecoInfo
{
    public string Logradouro { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string? Complemento { get; set; }
    public string Bairro { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string Cep { get; set; } = string.Empty;
    public string? Pais { get; set; }

    [JsonIgnore]
    public bool IsEmpty =>
        string.IsNullOrEmpty(Logradouro) &&
        string.IsNullOrEmpty(Numero) &&
        string.IsNullOrEmpty(Complemento) &&
        string.IsNullOrEmpty(Bairro) &&
        string.IsNullOrEmpty(Cidade) &&
        string.IsNullOrEmpty(Estado) &&
        string.IsNullOrEmpty(Cep) &&
        string.IsNullOrEmpty(Pais);
}
