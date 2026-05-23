namespace FirstWebApi.Application.DTOs.Request;

public class LoginRequest
{
    public string Email { get; init; } = string.Empty;
    public string Senha { get; init; } = string.Empty;
}
