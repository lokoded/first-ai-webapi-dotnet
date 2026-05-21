namespace FirstWebApi.Application.DTOs.Response;

public class UserResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Cpf { get; set; }
    public string? Rg { get; set; }
    public EnderecoInfo? Endereco { get; set; }
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
