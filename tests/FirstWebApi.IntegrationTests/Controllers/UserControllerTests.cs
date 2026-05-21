using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FirstWebApi.Application.DTOs.Request;
using FirstWebApi.Application.DTOs.Response;
using Microsoft.AspNetCore.Mvc.Testing;
using FluentAssertions;

namespace FirstWebApi.IntegrationTests.Controllers;

[Collection("Database")]
public class UserControllerTests : IClassFixture<FirstWebApiFactory>
{
    private readonly HttpClient _client;
    private readonly FirstWebApiFactory _factory;

    public UserControllerTests(FirstWebApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }


    private async Task<string> GetTokenAsync()
    {
        var email = $"profile_{Guid.NewGuid()}@email.com";

        var register = new RegisterRequest
        {
            Nome = "Profile Test",
            UserName = $"profile_{Guid.NewGuid():N}"[..20],
            Email = email,
            Senha = "SenhaForte123",
            Cpf = "529.982.247-25"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(register),
            Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/auth/register", content);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync();
        var authResponse = JsonSerializer.Deserialize<AuthResponse>(body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return authResponse!.Token;
    }

    [Fact]
    public async Task GetMe_ComTokenValido_DeveRetornar200()
    {
        var token = await GetTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/users/me");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        var userResponse = JsonSerializer.Deserialize<UserResponse>(body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        userResponse.Should().NotBeNull();
        userResponse!.Nome.Should().Be("Profile Test");
        userResponse.Email.Should().Contain("@email.com");
    }

    [Fact]
    public async Task GetMe_SemToken_DeveRetornar401()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.GetAsync("/api/users/me");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMe_ComTokenInvalido_DeveRetornar401()
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "token-falso-invalido");

        var response = await _client.GetAsync("/api/users/me");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }
}
