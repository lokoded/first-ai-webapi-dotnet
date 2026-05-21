using FirstWebApi.Application.DTOs;

namespace FirstWebApi.Application.DTOs.Request;

public class RegisterRequest
{
    public string Nome { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public string? Cpf { get; set; }
    public string? Rg { get; set; }
    public EnderecoInfo? Endereco { get; set; }
}
