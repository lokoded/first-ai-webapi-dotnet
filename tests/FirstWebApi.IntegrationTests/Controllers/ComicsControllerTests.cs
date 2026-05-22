using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FirstWebApi.Application.DTOs.Request;
using FirstWebApi.Application.DTOs.Response;
using FirstWebApi.Domain.Entities;
using FirstWebApi.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;

namespace FirstWebApi.IntegrationTests.Controllers;

[Collection("Database")]
public class ComicsControllerTests : IClassFixture<FirstWebApiFactory>
{
    private readonly HttpClient _client;
    private readonly FirstWebApiFactory _factory;

    public ComicsControllerTests(FirstWebApiFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    private async Task<string> RegisterAndGetTokenAsync()
    {
        var email = $"comics_{Guid.NewGuid()}@email.com";
        var register = new RegisterRequest
        {
            Nome = "Comics Test",
            UserName = $"comics_{Guid.NewGuid():N}"[..20],
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

    private Guid SeedComicType()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var nome = $"HQ_{Guid.NewGuid():N}"[..20];
        var comicType = new ComicType(nome);
        db.Set<ComicType>().Add(comicType);
        db.SaveChanges();
        return comicType.Id;
    }

    [Fact]
    public async Task PostComic_WithValidData_ShouldReturn201()
    {
        var token = await RegisterAndGetTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var comicTypeId = SeedComicType();

        var request = new ComicRequest
        {
            Titulo = "Batman: Ano Um",
            WebUrl = "https://exemplo.com/batman-ano-um",
            ComicTypeId = comicTypeId
        };

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/comics", content);
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created, body);

        var comic = JsonSerializer.Deserialize<ComicResponse>(body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        comic.Should().NotBeNull();
        comic!.Titulo.Should().Be("Batman: Ano Um");
    }

    [Fact]
    public async Task GetComics_WithoutToken_ShouldReturn401()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.GetAsync("/api/comics");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PostComic_WithEmptyTitle_ShouldReturn400()
    {
        var token = await RegisterAndGetTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var request = new ComicRequest
        {
            Titulo = "",
            WebUrl = "https://exemplo.com/invalida",
            ComicTypeId = Guid.NewGuid()
        };

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/comics", content);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostComic_WithInvalidWebUrl_ShouldReturn400()
    {
        var token = await RegisterAndGetTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var request = new ComicRequest
        {
            Titulo = "URL Inválida",
            WebUrl = "nao-e-uma-url",
            ComicTypeId = Guid.NewGuid()
        };

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/comics", content);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostComic_WithInvalidComicTypeId_ShouldReturn404()
    {
        var token = await RegisterAndGetTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var request = new ComicRequest
        {
            Titulo = "Comic Inválida",
            WebUrl = "https://exemplo.com/invalida",
            ComicTypeId = Guid.NewGuid()
        };

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/comics", content);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetComicById_WithNonExistentId_ShouldReturn404()
    {
        var token = await RegisterAndGetTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync($"/api/comics/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateComic_WithNonExistentId_ShouldReturn404()
    {
        var token = await RegisterAndGetTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var request = new ComicRequest
        {
            Titulo = "Updated",
            WebUrl = "https://exemplo.com/updated",
            ComicTypeId = Guid.NewGuid()
        };

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8, "application/json");

        var response = await _client.PutAsync($"/api/comics/{Guid.NewGuid()}", content);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }
}
