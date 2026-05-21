using System.Text;
using System.Text.Json;
using FirstWebApi.Application.DTOs.Request;
using FirstWebApi.Application.DTOs.Response;
using Microsoft.AspNetCore.Mvc.Testing;
using FluentAssertions;

namespace FirstWebApi.IntegrationTests.Controllers;

[Collection("Database")]
public class AuthControllerTests : IClassFixture<FirstWebApiFactory>
{
    private readonly HttpClient _client;

    public AuthControllerTests(FirstWebApiFactory factory)
    {
        _client = factory.CreateClient();
    }


    [Fact]
    public async Task PostRegister_ComDadosValidos_DeveRetornar200()
    {
        var request = new RegisterRequest
        {
            Nome = "Usuário Teste",
            UserName = $"teste_{Guid.NewGuid():N}"[..20],
            Email = $"teste_{Guid.NewGuid()}@email.com",
            Senha = "SenhaForte123",
            Cpf = "529.982.247-25"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        var response = await _client.PostAsync("/api/auth/register", content);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        var authResponse = JsonSerializer.Deserialize<AuthResponse>(body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        authResponse.Should().NotBeNull();
        authResponse!.Token.Should().NotBeNullOrEmpty();
        authResponse.Email.Should().Be(request.Email);
    }

    [Fact]
    public async Task PostRegister_ComEmailDuplicado_DeveRetornar409()
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

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        await _client.PostAsync("/api/auth/register", content);
        var response = await _client.PostAsync("/api/auth/register", content);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task PostLogin_ComCredenciaisValidas_DeveRetornar200()
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

        var registerContent = new StringContent(
            JsonSerializer.Serialize(register),
            Encoding.UTF8, "application/json");

        await _client.PostAsync("/api/auth/register", registerContent);

        var login = new LoginRequest
        {
            Email = email,
            Senha = "SenhaForte123"
        };

        var loginContent = new StringContent(
            JsonSerializer.Serialize(login),
            Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/auth/login", loginContent);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        var authResponse = JsonSerializer.Deserialize<AuthResponse>(body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        authResponse.Should().NotBeNull();
        authResponse!.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task PostLogin_ComSenhaInvalida_DeveRetornar401()
    {
        var login = new LoginRequest
        {
            Email = "naoexiste@email.com",
            Senha = "SenhaErrada"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(login),
            Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/auth/login", content);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PostRegister_ComCpfInvalido_DeveRetornar400()
    {
        var request = new RegisterRequest
        {
            Nome = "CPF Inválido",
            UserName = $"cpf_invalido_{Guid.NewGuid():N}"[..20],
            Email = $"cpf_invalido_{Guid.NewGuid()}@email.com",
            Senha = "SenhaForte123",
            Cpf = "123.456.789-00"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/auth/register", content);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostRegister_ComSenhaFraca_DeveRetornar400()
    {
        var request = new RegisterRequest
        {
            Nome = "Senha Fraca",
            UserName = $"senha_fraca_{Guid.NewGuid():N}"[..20],
            Email = $"senha_fraca_{Guid.NewGuid()}@email.com",
            Senha = "fraca",
            Cpf = "529.982.247-25"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/auth/register", content);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostRegister_ComEmailInvalido_DeveRetornar400()
    {
        var request = new RegisterRequest
        {
            Nome = "Email Inválido",
            UserName = $"email_invalido_{Guid.NewGuid():N}"[..20],
            Email = "invalido",
            Senha = "SenhaForte123",
            Cpf = "529.982.247-25"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/auth/register", content);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostRegister_SemCpfESemRg_DeveRetornar400()
    {
        var request = new RegisterRequest
        {
            Nome = "Sem CPF RG",
            UserName = $"sem_cpf_rg_{Guid.NewGuid():N}"[..20],
            Email = $"sem_cpf_rg_{Guid.NewGuid()}@email.com",
            Senha = "SenhaForte123"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/auth/register", content);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }
}
