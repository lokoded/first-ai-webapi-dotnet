using FirstWebApi.Application.DTOs;

namespace FirstWebApi.Application.DTOs.Request;

public class RegisterRequest
{
    public string Nome { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Senha { get; init; } = string.Empty;
    public string? Cpf { get; init; }
    public string? Rg { get; init; }
    public EnderecoInfo? Endereco { get; init; }
}
