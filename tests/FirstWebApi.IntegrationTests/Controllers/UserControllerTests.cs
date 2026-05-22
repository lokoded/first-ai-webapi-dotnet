using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FirstWebApi.Application.DTOs.Request;
using FirstWebApi.Application.DTOs.Response;
using FirstWebApi.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
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
    public async Task GetMe_WithValidToken_ShouldReturn200()
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
    public async Task GetMe_WithoutToken_ShouldReturn401()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.GetAsync("/api/users/me");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMe_WithInvalidToken_ShouldReturn401()
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "token-falso-invalido");

        var response = await _client.GetAsync("/api/users/me");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetFullProfile_WithValidPassword_ShouldReturn200()
    {
        var email = $"full_{Guid.NewGuid()}@email.com";
        var password = "SenhaForte123";

        var register = new RegisterRequest
        {
            Nome = "Full Profile",
            UserName = $"full_{Guid.NewGuid():N}"[..20],
            Email = email,
            Senha = password,
            Cpf = "529.982.247-25"
        };

        var registerContent = new StringContent(
            JsonSerializer.Serialize(register),
            Encoding.UTF8, "application/json");

        var registerResponse = await _client.PostAsync("/api/auth/register", registerContent);
        var registerBody = await registerResponse.Content.ReadAsStringAsync();
        var authResponse = JsonSerializer.Deserialize<AuthResponse>(registerBody,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authResponse!.Token);

        var fullRequest = new FullProfileRequest
        {
            Senha = password
        };

        var fullContent = new StringContent(
            JsonSerializer.Serialize(fullRequest),
            Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/users/me/full", fullContent);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        var userResponse = JsonSerializer.Deserialize<UserResponse>(body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        userResponse.Should().NotBeNull();
        userResponse!.Nome.Should().Be("Full Profile");
        userResponse.Cpf.Should().Be("52998224725");
        userResponse.HasFullData.Should().BeTrue();
    }

    [Fact]
    public async Task GetFullProfile_WithWrongPassword_ShouldReturn403()
    {
        var email = $"fullwrong_{Guid.NewGuid()}@email.com";

        var register = new RegisterRequest
        {
            Nome = "Full Wrong",
            UserName = $"fullwrong_{Guid.NewGuid():N}"[..20],
            Email = email,
            Senha = "SenhaForte123",
            Rg = "12.345.678-9"
        };

        var registerContent = new StringContent(
            JsonSerializer.Serialize(register),
            Encoding.UTF8, "application/json");

        var registerResponse = await _client.PostAsync("/api/auth/register", registerContent);
        var registerBody = await registerResponse.Content.ReadAsStringAsync();
        var authResponse = JsonSerializer.Deserialize<AuthResponse>(registerBody,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authResponse!.Token);

        var fullRequest = new FullProfileRequest
        {
            Senha = "SenhaErrada123"
        };

        var fullContent = new StringContent(
            JsonSerializer.Serialize(fullRequest),
            Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/users/me/full", fullContent);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
    }
}
