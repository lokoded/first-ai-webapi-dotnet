using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FirstWebApi.Application.DTOs.Request;
using FirstWebApi.Application.DTOs.Response;
using FluentAssertions;

namespace FirstWebApi.IntegrationTests.Controllers;

public class UserControllerTests(FirstWebApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetMe_WithValidToken_ShouldReturn200()
    {
        var token = await RegisterAndGetTokenAsync("profile");
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await Client.GetAsync("/api/users/me");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        var userResponse = JsonSerializer.Deserialize<UserResponse>(body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        userResponse.Should().NotBeNull();
        userResponse!.Nome.Should().Be("profile Test");
        userResponse.Email.Should().Contain("@email.com");
    }

    [Fact]
    public async Task GetMe_WithoutToken_ShouldReturn401()
    {
        Client.DefaultRequestHeaders.Authorization = null;

        var response = await Client.GetAsync("/api/users/me");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMe_WithInvalidToken_ShouldReturn401()
    {
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "token-falso-invalido");

        var response = await Client.GetAsync("/api/users/me");

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

        var registerContent = JsonContent(register);

        var registerResponse = await Client.PostAsync("/api/auth/register", registerContent);
        var registerBody = await registerResponse.Content.ReadAsStringAsync();
        var authResponse = JsonSerializer.Deserialize<AuthResponse>(registerBody,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authResponse!.Token);

        var fullRequest = new FullProfileRequest
        {
            Senha = password
        };

        var fullContent = JsonContent(fullRequest);

        var response = await Client.PostAsync("/api/users/me/full", fullContent);

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
    public async Task GetFullProfile_WithWrongPassword_ShouldReturn400()
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

        var registerContent = JsonContent(register);

        var registerResponse = await Client.PostAsync("/api/auth/register", registerContent);
        var registerBody = await registerResponse.Content.ReadAsStringAsync();
        var authResponse = JsonSerializer.Deserialize<AuthResponse>(registerBody,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authResponse!.Token);

        var fullRequest = new FullProfileRequest
        {
            Senha = "SenhaErrada123"
        };

        var fullContent = JsonContent(fullRequest);

        var response = await Client.PostAsync("/api/users/me/full", fullContent);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }
}
