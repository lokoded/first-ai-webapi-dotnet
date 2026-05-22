using System.Security.Claims;
using FirstWebApi.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace FirstWebApi.UnitTests.Infrastructure;

public class TokenServiceTests
{
    private readonly TokenService _tokenService;
    private readonly IConfiguration _configuration;

    public TokenServiceTests()
    {
        var configData = new Dictionary<string, string?>
        {
            { "Jwt:SecretKey", "X7k9p2m4q8w3e6r1t5y0u2i4o7p9a3s6d8f1g2h5j7k0l2z4x6c8v1b3n5m7" },
            { "Jwt:Issuer", "FirstWebApi" },
            { "Jwt:Audience", "FirstWebApiClient" }
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        _tokenService = new TokenService(_configuration);
    }

    [Fact]
    public void GenerateToken_WithValidData_ShouldReturnJwtToken()
    {
        var token = _tokenService.GenerateToken(
            Guid.NewGuid(), "teste@email.com", "Teste", ["User"]);

        token.Should().NotBeNullOrEmpty();
        token.Split('.').Should().HaveCount(3);
    }

    [Fact]
    public void GenerateToken_ShouldContainCorrectClaims()
    {
        var userId = Guid.NewGuid();
        var token = _tokenService.GenerateToken(
            userId, "teste@email.com", "Teste", ["User", "Admin"]);

        var principal = _tokenService.ValidateToken(token);

        principal.Should().NotBeNull();
        principal!.FindFirst(ClaimTypes.NameIdentifier)?.Value.Should().Be(userId.ToString());
        principal.FindFirst(ClaimTypes.Email)?.Value.Should().Be("teste@email.com");
        principal.FindAll(ClaimTypes.Role).Select(c => c.Value).Should().Contain(["User", "Admin"]);
    }

    [Fact]
    public void ValidateToken_WithInvalidToken_ShouldReturnNull()
    {
        var principal = _tokenService.ValidateToken("token-invalido");
        principal.Should().BeNull();
    }


}
