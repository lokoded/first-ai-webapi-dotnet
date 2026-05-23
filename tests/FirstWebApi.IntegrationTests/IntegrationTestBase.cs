using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FirstWebApi.Application.DTOs.Request;
using FirstWebApi.Application.DTOs.Response;
using FirstWebApi.Domain.Entities;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;

namespace FirstWebApi.IntegrationTests;

[Collection("Database")]
public abstract class IntegrationTestBase : IClassFixture<FirstWebApiFactory>
{
    protected readonly FirstWebApiFactory Factory;
    protected readonly HttpClient Client;

    protected IntegrationTestBase(FirstWebApiFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    protected async Task<string> RegisterAndGetTokenAsync(string prefix)
    {
        var email = $"{prefix}_{Guid.NewGuid()}@email.com";
        var register = new RegisterRequest
        {
            Nome = $"{prefix} Test",
            UserName = $"{prefix}_{Guid.NewGuid():N}"[..20],
            Email = email,
            Senha = "SenhaForte123",
            Cpf = "529.982.247-25"
        };

        var content = JsonContent(register);

        var response = await Client.PostAsync("/api/auth/register", content);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync();
        var authResponse = JsonSerializer.Deserialize<AuthResponse>(body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return authResponse!.Token;
    }

    protected async Task<string> RegisterAndGetAdminTokenAsync(string prefix)
    {
        var email = $"{prefix}_{Guid.NewGuid()}@email.com";
        var register = new RegisterRequest
        {
            Nome = $"{prefix} Test",
            UserName = $"{prefix}_{Guid.NewGuid():N}"[..20],
            Email = email,
            Senha = "SenhaForte123",
            Cpf = "529.982.247-25"
        };

        var content = JsonContent(register);

        var response = await Client.PostAsync("/api/auth/register", content);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync();
        var authResponse = JsonSerializer.Deserialize<AuthResponse>(body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        using (var scope = Factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var user = await userManager.FindByEmailAsync(email);
            if (user != null)
                await userManager.AddToRoleAsync(user, "Admin");
        }

        var login = new LoginRequest { Email = email, Senha = "SenhaForte123" };
        var loginContent = JsonContent(login);
        var loginResponse = await Client.PostAsync("/api/auth/login", loginContent);
        loginResponse.EnsureSuccessStatusCode();

        var loginBody = await loginResponse.Content.ReadAsStringAsync();
        var loginAuthResponse = JsonSerializer.Deserialize<AuthResponse>(loginBody,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return loginAuthResponse!.Token;
    }

    protected static StringContent JsonContent<T>(T value) =>
        new(JsonSerializer.Serialize(value), Encoding.UTF8, "application/json");

    protected void SetAuthHeader(string token)
    {
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }
}
