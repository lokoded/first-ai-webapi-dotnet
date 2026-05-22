using FirstWebApi.Application.DTOs.Request;
using FirstWebApi.Application.DTOs.Response;
using FirstWebApi.Application.Interfaces;
using FirstWebApi.Application.Services;
using FirstWebApi.Domain.Entities;
using FirstWebApi.Domain.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;

namespace FirstWebApi.UnitTests.Application;

public class AuthServiceTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IEncryptionService> _encryptionMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<AuthService>> _loggerMock;
    private readonly Mock<IAddressRepository> _addressRepoMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepoMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _userManagerMock = new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(), null!, null!, null!, null!, null!, null!, null!, null!);
        _userRepoMock = new Mock<IUserRepository>();
        _tokenServiceMock = new Mock<ITokenService>();
        _tokenServiceMock.Setup(t => t.GenerateRefreshToken())
            .Returns(("fake-refresh-token", "fake-hash"));
        _encryptionMock = new Mock<IEncryptionService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<AuthService>>();
        _addressRepoMock = new Mock<IAddressRepository>();
        _refreshTokenRepoMock = new Mock<IRefreshTokenRepository>();

        _authService = new AuthService(
            _userManagerMock.Object,
            _userRepoMock.Object,
            _tokenServiceMock.Object,
            _encryptionMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object,
            _addressRepoMock.Object,
            _refreshTokenRepoMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_WithValidData_ShouldReturnToken()
    {
        _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock.Setup(m => m.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock.Setup(m => m.GetRolesAsync(It.IsAny<User>()))
            .ReturnsAsync(["User"]);

        _userRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new User("João", "joao_123", "joao@email.com"));

        _tokenServiceMock.Setup(t => t.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<string>>()))
            .Returns("fake-jwt-token");

        var request = new RegisterRequest
        {
            Nome = "João",
            UserName = "joao_123",
            Email = "joao@email.com",
            Senha = "SenhaForte123",
            Cpf = "529.982.247-25"
        };

        var result = await _authService.RegisterAsync(request);

        result.Should().NotBeNull();
        result.Token.Should().Be("fake-jwt-token");
        result.Email.Should().Be("joao@email.com");
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ShouldThrowException()
    {
        _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(new User("João", "joao_123", "joao@email.com"));

        var request = new RegisterRequest
        {
            Nome = "João",
            UserName = "joao_123",
            Email = "joao@email.com",
            Senha = "SenhaForte123",
            Rg = "12.345.678-9"
        };

        Func<Task> act = () => _authService.RegisterAsync(request);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Email já cadastrado.");
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnToken()
    {
        var user = new User("João", "joao_123", "joao@email.com");

        _userRepoMock.Setup(r => r.GetByEmailAsync("joao@email.com"))
            .ReturnsAsync(user);

        _userManagerMock.Setup(m => m.CheckPasswordAsync(user, "SenhaForte123"))
            .ReturnsAsync(true);

        _userManagerMock.Setup(m => m.GetRolesAsync(user))
            .ReturnsAsync(["User"]);

        _tokenServiceMock.Setup(t => t.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<string>>()))
            .Returns("fake-jwt-token");

        var request = new LoginRequest
        {
            Email = "joao@email.com",
            Senha = "SenhaForte123"
        };

        var result = await _authService.LoginAsync(request);

        result.Should().NotBeNull();
        result.Token.Should().Be("fake-jwt-token");
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ShouldThrowException()
    {
        var user = new User("João", "joao_123", "joao@email.com");

        _userRepoMock.Setup(r => r.GetByEmailAsync("joao@email.com"))
            .ReturnsAsync(user);

        _userManagerMock.Setup(m => m.CheckPasswordAsync(user, It.IsAny<string>()))
            .ReturnsAsync(false);

        var request = new LoginRequest
        {
            Email = "joao@email.com",
            Senha = "SenhaErrada"
        };

        Func<Task> act = () => _authService.LoginAsync(request);
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Email ou senha inválidos.");
    }

    [Fact]
    public async Task RefreshTokenAsync_WithValidToken_ShouldReturnNewToken()
    {
        var user = new User("João", "joao_123", "joao@email.com");
        var storedToken = new RefreshToken(user.Id, "token-hash", DateTime.UtcNow.AddDays(7));

        _tokenServiceMock.Setup(t => t.HashToken(It.IsAny<string>()))
            .Returns("token-hash");
        _refreshTokenRepoMock.Setup(r => r.GetByTokenHashAsync("token-hash"))
            .ReturnsAsync(storedToken);
        _userRepoMock.Setup(r => r.GetByIdAsync(user.Id))
            .ReturnsAsync(user);
        _tokenServiceMock.Setup(t => t.GenerateRefreshToken())
            .Returns(("new-refresh-token", "new-hash"));
        _tokenServiceMock.Setup(t => t.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<string>>()))
            .Returns("fake-jwt-token");
        _userManagerMock.Setup(m => m.GetRolesAsync(It.IsAny<User>()))
            .ReturnsAsync(["User"]);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        var result = await _authService.RefreshTokenAsync("valid-refresh-token");

        result.Should().NotBeNull();
        result.Token.Should().Be("fake-jwt-token");
        result.RefreshToken.Should().Be("new-refresh-token");
    }

    [Fact]
    public async Task RefreshTokenAsync_WithNonExistentToken_ShouldThrowException()
    {
        _tokenServiceMock.Setup(t => t.HashToken(It.IsAny<string>()))
            .Returns("invalid-hash");
        _refreshTokenRepoMock.Setup(r => r.GetByTokenHashAsync("invalid-hash"))
            .ReturnsAsync((RefreshToken?)null);

        Func<Task> act = () => _authService.RefreshTokenAsync("invalid-token");
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Refresh token inválido ou expirado.");
    }

    [Fact]
    public async Task RefreshTokenAsync_WithExpiredToken_ShouldThrowException()
    {
        var storedToken = new RefreshToken(Guid.NewGuid(), "token-hash", DateTime.UtcNow.AddDays(-1));

        _tokenServiceMock.Setup(t => t.HashToken(It.IsAny<string>()))
            .Returns("token-hash");
        _refreshTokenRepoMock.Setup(r => r.GetByTokenHashAsync("token-hash"))
            .ReturnsAsync(storedToken);

        Func<Task> act = () => _authService.RefreshTokenAsync("expired-token");
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Refresh token inválido ou expirado.");
    }

    [Fact]
    public async Task RefreshTokenAsync_WithRevokedToken_ShouldThrowException()
    {
        var storedToken = new RefreshToken(Guid.NewGuid(), "token-hash", DateTime.UtcNow.AddDays(7));
        storedToken.Revoke();

        _tokenServiceMock.Setup(t => t.HashToken(It.IsAny<string>()))
            .Returns("token-hash");
        _refreshTokenRepoMock.Setup(r => r.GetByTokenHashAsync("token-hash"))
            .ReturnsAsync(storedToken);
        _refreshTokenRepoMock.Setup(r => r.GetActiveByUserIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync([]);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        Func<Task> act = () => _authService.RefreshTokenAsync("revoked-token");
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Refresh token inválido ou expirado.");
    }

    [Fact]
    public async Task RevokeRefreshTokensAsync_ShouldRevokeActiveTokens()
    {
        var userId = Guid.NewGuid();
        var token1 = new RefreshToken(userId, "hash1", DateTime.UtcNow.AddDays(7));
        var token2 = new RefreshToken(userId, "hash2", DateTime.UtcNow.AddDays(7));

        _refreshTokenRepoMock.Setup(r => r.GetActiveByUserIdAsync(userId))
            .ReturnsAsync([token1, token2]);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(2);

        await _authService.RevokeRefreshTokensAsync(userId);

        token1.IsRevoked.Should().BeTrue();
        token2.IsRevoked.Should().BeTrue();
        _refreshTokenRepoMock.Verify(r => r.Update(token1), Times.Once);
        _refreshTokenRepoMock.Verify(r => r.Update(token2), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

}
