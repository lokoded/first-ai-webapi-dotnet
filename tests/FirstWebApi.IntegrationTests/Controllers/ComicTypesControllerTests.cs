using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FirstWebApi.Application.DTOs.Request;
using FirstWebApi.Application.DTOs.Response;
using Microsoft.AspNetCore.Mvc.Testing;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using FirstWebApi.Domain.Entities;

namespace FirstWebApi.IntegrationTests.Controllers;

[Collection("Database")]
public class ComicTypesControllerTests : IClassFixture<FirstWebApiFactory>
{
    private readonly HttpClient _client;
    private readonly FirstWebApiFactory _factory;

    public ComicTypesControllerTests(FirstWebApiFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    private async Task<string> RegisterAndGetTokenAsync()
    {
        var email = $"comictypes_{Guid.NewGuid()}@email.com";
        var register = new RegisterRequest
        {
            Nome = "ComicTypes Test",
            UserName = $"ctypes_{Guid.NewGuid():N}"[..20],
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
    public async Task GetComicTypes_WithValidToken_ShouldReturn200()
    {
        var token = await RegisterAndGetTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/comic-types");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        var types = JsonSerializer.Deserialize<List<ComicTypeResponse>>(body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        types.Should().NotBeNull();
    }

    [Fact]
    public async Task GetComicTypes_WithoutToken_ShouldReturn401()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.GetAsync("/api/comic-types");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AdminPostComicType_WithoutAdminRole_ShouldReturn403()
    {
        var token = await RegisterAndGetTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var request = new ComicTypeRequest { Nome = "Mangá" };

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/admin/comic-types", content);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdminPostComicType_WithoutToken_ShouldReturn401()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var request = new ComicTypeRequest { Nome = "Mangá" };
        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/admin/comic-types", content);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AdminDeleteComicType_WithoutToken_ShouldReturn401()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.DeleteAsync($"/api/admin/comic-types/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    private async Task<string> RegisterAndGetAdminTokenAsync()
    {
        var email = $"admin_{Guid.NewGuid()}@email.com";
        var register = new RegisterRequest
        {
            Nome = "Admin Test",
            UserName = $"admin_{Guid.NewGuid():N}"[..20],
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

        using (var scope = _factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var user = await userManager.FindByEmailAsync(email);
            if (user != null)
            {
                await userManager.AddToRoleAsync(user, "Admin");
            }
        }

        var login = new LoginRequest { Email = email, Senha = "SenhaForte123" };
        var loginContent = new StringContent(JsonSerializer.Serialize(login), Encoding.UTF8, "application/json");
        var loginResponse = await _client.PostAsync("/api/auth/login", loginContent);
        loginResponse.EnsureSuccessStatusCode();

        var loginBody = await loginResponse.Content.ReadAsStringAsync();
        var loginAuthResponse = JsonSerializer.Deserialize<AuthResponse>(loginBody,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return loginAuthResponse!.Token;
    }

    [Fact]
    public async Task AdminPostComicType_WithAdminUser_ShouldReturn201()
    {
        var token = await RegisterAndGetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var request = new ComicTypeRequest { Nome = $"Mangá_{Guid.NewGuid():N}"[..15] };

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/admin/comic-types", content);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
    }
}
