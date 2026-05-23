using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FirstWebApi.Application.DTOs.Request;
using FirstWebApi.Application.DTOs.Response;
using FluentAssertions;

namespace FirstWebApi.IntegrationTests.Controllers;

public class ComicTypesControllerTests(FirstWebApiFactory factory) : IntegrationTestBase(factory)
{

    [Fact]
    public async Task GetComicTypes_WithValidToken_ShouldReturn200()
    {
        var token = await RegisterAndGetTokenAsync("comictypes");
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await Client.GetAsync("/api/comic-types");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        var types = JsonSerializer.Deserialize<List<ComicTypeResponse>>(body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        types.Should().NotBeNull();
    }

    [Fact]
    public async Task GetComicTypes_WithoutToken_ShouldReturn401()
    {
        Client.DefaultRequestHeaders.Authorization = null;

        var response = await Client.GetAsync("/api/comic-types");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AdminPostComicType_WithoutAdminRole_ShouldReturn403()
    {
        var token = await RegisterAndGetTokenAsync("comictypes");
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var request = new ComicTypeRequest { Nome = "Mangá" };

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8, "application/json");

        var response = await Client.PostAsync("/api/admin/comic-types", content);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdminPostComicType_WithoutToken_ShouldReturn401()
    {
        Client.DefaultRequestHeaders.Authorization = null;

        var request = new ComicTypeRequest { Nome = "Mangá" };
        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8, "application/json");

        var response = await Client.PostAsync("/api/admin/comic-types", content);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AdminDeleteComicType_WithoutToken_ShouldReturn401()
    {
        Client.DefaultRequestHeaders.Authorization = null;

        var response = await Client.DeleteAsync($"/api/admin/comic-types/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AdminPostComicType_WithAdminUser_ShouldReturn201()
    {
        var token = await RegisterAndGetAdminTokenAsync("admin");
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var request = new ComicTypeRequest { Nome = $"Mangá_{Guid.NewGuid():N}"[..15] };

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8, "application/json");

        var response = await Client.PostAsync("/api/admin/comic-types", content);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
    }
}
