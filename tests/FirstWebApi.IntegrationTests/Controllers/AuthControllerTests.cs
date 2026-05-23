using System.Text;
using System.Text.Json;
using FirstWebApi.Application.DTOs.Request;
using FirstWebApi.Application.DTOs.Response;
using FluentAssertions;

namespace FirstWebApi.IntegrationTests.Controllers;

[Collection("Database")]
public class AuthControllerTests : IClassFixture<FirstWebApiFactory>
{
    private readonly HttpClient _client;

    public AuthControllerTests(FirstWebApiFactory factory)
    {
        var handler = factory.Server.CreateHandler();
        _client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
    }

    private static StringContent JsonContent<T>(T value) =>
        new(JsonSerializer.Serialize(value), Encoding.UTF8, "application/json");

    [Fact]
    public async Task PostRegister_WithValidData_ShouldReturn201()
    {
        var request = new RegisterRequest
        {
            Nome = "Usuário Teste",
            UserName = $"teste_{Guid.NewGuid():N}"[..20],
            Email = $"teste_{Guid.NewGuid()}@email.com",
            Senha = "SenhaForte123",
            Cpf = "529.982.247-25"
        };

        var content = JsonContent(request);

        var response = await _client.PostAsync("/api/auth/register", content);

        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        var authResponse = JsonSerializer.Deserialize<AuthResponse>(body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        authResponse.Should().NotBeNull();
        authResponse!.Token.Should().NotBeNullOrEmpty();
        authResponse.Email.Should().Be(request.Email);

    }

    [Fact]
    public async Task PostRegister_WithDuplicateEmail_ShouldReturn409()
    {
        var email = $"duplicado_{Guid.NewGuid()}@email.com";

        var request = new RegisterRequest
        {
            Nome = "Primeiro",
            UserName = $"primeiro_{Guid.NewGuid():N}"[..20],
            Email = email,
            Senha = "SenhaForte123",
            Cpf = "529.982.247-25"
        };

        var content = JsonContent(request);

        await _client.PostAsync("/api/auth/register", content);
        var response = await _client.PostAsync("/api/auth/register", content);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task PostLogin_WithValidCredentials_ShouldReturn200()
    {
        var email = $"login_{Guid.NewGuid()}@email.com";

        var register = new RegisterRequest
        {
            Nome = "Login Test",
            UserName = $"login_{Guid.NewGuid():N}"[..20],
            Email = email,
            Senha = "SenhaForte123",
            Cpf = "529.982.247-25"
        };

        var registerContent = JsonContent(register);

        await _client.PostAsync("/api/auth/register", registerContent);

        var login = new LoginRequest
        {
            Email = email,
            Senha = "SenhaForte123"
        };

        var loginContent = JsonContent(login);

        var response = await _client.PostAsync("/api/auth/login", loginContent);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        var authResponse = JsonSerializer.Deserialize<AuthResponse>(body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        authResponse.Should().NotBeNull();
        authResponse!.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task PostLogin_WithInvalidPassword_ShouldReturn401()
    {
        var login = new LoginRequest
        {
            Email = "naoexiste@email.com",
            Senha = "SenhaErrada"
        };

        var content = JsonContent(login);

        var response = await _client.PostAsync("/api/auth/login", content);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PostRegister_WithInvalidCpf_ShouldReturn400()
    {
        var request = new RegisterRequest
        {
            Nome = "CPF Inválido",
            UserName = $"cpf_invalido_{Guid.NewGuid():N}"[..20],
            Email = $"cpf_invalido_{Guid.NewGuid()}@email.com",
            Senha = "SenhaForte123",
            Cpf = "123.456.789-00"
        };

        var content = JsonContent(request);

        var response = await _client.PostAsync("/api/auth/register", content);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostRegister_WithWeakPassword_ShouldReturn400()
    {
        var request = new RegisterRequest
        {
            Nome = "Senha Fraca",
            UserName = $"senha_fraca_{Guid.NewGuid():N}"[..20],
            Email = $"senha_fraca_{Guid.NewGuid()}@email.com",
            Senha = "fraca",
            Cpf = "529.982.247-25"
        };

        var content = JsonContent(request);

        var response = await _client.PostAsync("/api/auth/register", content);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostRegister_WithInvalidEmail_ShouldReturn400()
    {
        var request = new RegisterRequest
        {
            Nome = "Email Inválido",
            UserName = $"email_invalido_{Guid.NewGuid():N}"[..20],
            Email = "invalido",
            Senha = "SenhaForte123",
            Cpf = "529.982.247-25"
        };

        var content = JsonContent(request);

        var response = await _client.PostAsync("/api/auth/register", content);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostRegister_WithoutCpfAndRg_ShouldReturn400()
    {
        var request = new RegisterRequest
        {
            Nome = "Sem CPF RG",
            UserName = $"sem_cpf_rg_{Guid.NewGuid():N}"[..20],
            Email = $"sem_cpf_rg_{Guid.NewGuid()}@email.com",
            Senha = "SenhaForte123"
        };

        var content = JsonContent(request);

        var response = await _client.PostAsync("/api/auth/register", content);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostRefresh_WithValidToken_ShouldReturnNewTokens()
    {
        var email = $"refresh_{Guid.NewGuid()}@email.com";
        var register = new RegisterRequest
        {
            Nome = "Refresh Test",
            UserName = $"ref_{Guid.NewGuid():N}"[..20],
            Email = email,
            Senha = "SenhaForte123",
            Cpf = "529.982.247-25"
        };
        var registerContent = JsonContent(register);
        var registerResponse = await _client.PostAsync("/api/auth/register", registerContent);
        registerResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);

        var refreshToken = GetRefreshTokenFromResponse(registerResponse);
        refreshToken.Should().NotBeNullOrEmpty();

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh");
        request.Headers.Add("Cookie", $"RefreshToken={refreshToken}");
        var refreshResponse = await _client.SendAsync(request);

        refreshResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        
        var newRefreshToken = GetRefreshTokenFromResponse(refreshResponse);
        newRefreshToken.Should().NotBeNullOrEmpty();
        newRefreshToken.Should().NotBe(refreshToken);
    }

    [Fact]
    public async Task PostRevoke_WithAuthenticatedUser_ShouldRevokeTokens()
    {
        var email = $"revoke_{Guid.NewGuid()}@email.com";
        var register = new RegisterRequest
        {
            Nome = "Revoke Test",
            UserName = $"rev_{Guid.NewGuid():N}"[..20],
            Email = email,
            Senha = "SenhaForte123",
            Cpf = "529.982.247-25"
        };
        var registerContent = JsonContent(register);
        var registerResponse = await _client.PostAsync("/api/auth/register", registerContent);
        registerResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);

        var registerBody = await registerResponse.Content.ReadAsStringAsync();
        var authResponse = JsonSerializer.Deserialize<AuthResponse>(registerBody,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        authResponse.Should().NotBeNull();

        var refreshToken = GetRefreshTokenFromResponse(registerResponse);
        refreshToken.Should().NotBeNullOrEmpty();

        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResponse!.Token);

        var response = await _client.PostAsync("/api/auth/revoke", null);
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh");
        request.Headers.Add("Cookie", $"RefreshToken={refreshToken}");
        var refreshResponse = await _client.SendAsync(request);

        refreshResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    private static string? GetRefreshTokenFromResponse(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var cookies))
            return null;

        foreach (var cookie in cookies)
        {
            var parts = cookie.Split(';')[0].Split('=', 2);
            if (parts.Length == 2 && parts[0].Trim() == "RefreshToken")
                return parts[1];
        }
        return null;
    }
}
