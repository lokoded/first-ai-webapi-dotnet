using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FirstWebApi.Application.DTOs.Request;
using FirstWebApi.Application.DTOs.Response;
using FirstWebApi.Domain.Entities;
using FirstWebApi.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;

namespace FirstWebApi.IntegrationTests.Controllers;

public class ComicsControllerTests(FirstWebApiFactory factory) : IntegrationTestBase(factory)
{
    private Guid SeedComicType()
    {
        using var scope = Factory.Services.CreateScope();
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
        var token = await RegisterAndGetTokenAsync("comics");
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var comicTypeId = SeedComicType();

        var request = new ComicRequest
        {
            Titulo = "Batman: Ano Um",
            WebUrl = "https://exemplo.com/batman-ano-um",
            ComicTypeId = comicTypeId
        };

        var content = JsonContent(request);

        var response = await Client.PostAsync("/api/comics", content);
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
        Client.DefaultRequestHeaders.Authorization = null;

        var response = await Client.GetAsync("/api/comics");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PostComic_WithEmptyTitle_ShouldReturn400()
    {
        var token = await RegisterAndGetTokenAsync("comics");
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var comicTypeId = SeedComicType();

        var request = new ComicRequest
        {
            Titulo = "",
            WebUrl = "https://exemplo.com/invalida",
            ComicTypeId = comicTypeId
        };

        var content = JsonContent(request);

        var response = await Client.PostAsync("/api/comics", content);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostComic_WithInvalidWebUrl_ShouldReturn400()
    {
        var token = await RegisterAndGetTokenAsync("comics");
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var comicTypeId = SeedComicType();

        var request = new ComicRequest
        {
            Titulo = "URL Inválida",
            WebUrl = "nao-e-uma-url",
            ComicTypeId = comicTypeId
        };

        var content = JsonContent(request);

        var response = await Client.PostAsync("/api/comics", content);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostComic_WithInvalidComicTypeId_ShouldReturn404()
    {
        var token = await RegisterAndGetTokenAsync("comics");
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var request = new ComicRequest
        {
            Titulo = "Comic Inválida",
            WebUrl = "https://exemplo.com/invalida",
            ComicTypeId = Guid.NewGuid()
        };

        var content = JsonContent(request);

        var response = await Client.PostAsync("/api/comics", content);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetComicById_WithNonExistentId_ShouldReturn404()
    {
        var token = await RegisterAndGetTokenAsync("comics");
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await Client.GetAsync($"/api/comics/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateComic_WithNonExistentId_ShouldReturn404()
    {
        var token = await RegisterAndGetTokenAsync("comics");
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var request = new ComicRequest
        {
            Titulo = "Updated",
            WebUrl = "https://exemplo.com/updated",
            ComicTypeId = Guid.NewGuid()
        };

        var content = JsonContent(request);

        var response = await Client.PutAsync($"/api/comics/{Guid.NewGuid()}", content);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetComics_WithValidToken_ShouldReturn200()
    {
        var token = await RegisterAndGetTokenAsync("comics");
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await Client.GetAsync("/api/comics?page=1&pageSize=10");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetComicById_WithExistingId_ShouldReturn200()
    {
        var token = await RegisterAndGetTokenAsync("comics");
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var comicTypeId = SeedComicType();
        var createRequest = new ComicRequest
        {
            Titulo = "Get By Id Test",
            WebUrl = "https://exemplo.com/get-by-id",
            ComicTypeId = comicTypeId
        };

        var createContent = JsonContent(createRequest);

        var createResponse = await Client.PostAsync("/api/comics", createContent);
        var createBody = await createResponse.Content.ReadAsStringAsync();
        var created = JsonSerializer.Deserialize<ComicResponse>(createBody,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        var response = await Client.GetAsync($"/api/comics/{created!.Id}");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        var comic = JsonSerializer.Deserialize<ComicResponse>(body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        comic!.Titulo.Should().Be("Get By Id Test");
    }

    [Fact]
    public async Task UpdateComic_WithValidData_ShouldReturn204()
    {
        var token = await RegisterAndGetTokenAsync("comics");
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var comicTypeId = SeedComicType();
        var createRequest = new ComicRequest
        {
            Titulo = "Original Title",
            WebUrl = "https://exemplo.com/original",
            ComicTypeId = comicTypeId
        };

        var createContent = JsonContent(createRequest);

        var createResponse = await Client.PostAsync("/api/comics", createContent);
        var createBody = await createResponse.Content.ReadAsStringAsync();
        var created = JsonSerializer.Deserialize<ComicResponse>(createBody,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        var updateRequest = new ComicRequest
        {
            Titulo = "Updated Title",
            WebUrl = "https://exemplo.com/updated",
            ComicTypeId = comicTypeId
        };

        var updateContent = JsonContent(updateRequest);

        var response = await Client.PutAsync($"/api/comics/{created!.Id}", updateContent);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteComic_WithExistingId_ShouldReturn204()
    {
        var token = await RegisterAndGetTokenAsync("comics");
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var comicTypeId = SeedComicType();
        var createRequest = new ComicRequest
        {
            Titulo = "To Delete",
            WebUrl = "https://exemplo.com/to-delete",
            ComicTypeId = comicTypeId
        };

        var createContent = JsonContent(createRequest);

        var createResponse = await Client.PostAsync("/api/comics", createContent);
        var createBody = await createResponse.Content.ReadAsStringAsync();
        var created = JsonSerializer.Deserialize<ComicResponse>(createBody,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        var response = await Client.DeleteAsync($"/api/comics/{created!.Id}");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);
    }
}
