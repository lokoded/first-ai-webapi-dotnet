using FirstWebApi.Application.DTOs.Request;
using FirstWebApi.Application.DTOs.Response;
using FirstWebApi.Application.Exceptions;
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
    private readonly Mock<ISensitiveDataService> _sensitiveDataMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<AuthService>> _loggerMock;
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
        _sensitiveDataMock = new Mock<ISensitiveDataService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<AuthService>>();
        _refreshTokenRepoMock = new Mock<IRefreshTokenRepository>();

        _authService = new AuthService(
            _userManagerMock.Object,
            _userRepoMock.Object,
            _tokenServiceMock.Object,
            _sensitiveDataMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object,
            _refreshTokenRepoMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_WithValidData_ShouldReturnToken()
    {
        _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock.Setup(m => m.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock.Setup(m => m.GetRolesAsync(It.IsAny<User>()))
            .ReturnsAsync(["User"]);

        _userRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
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
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Email já cadastrado.");
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnToken()
    {
        var user = new User("João", "joao_123", "joao@email.com");

        _userRepoMock.Setup(r => r.GetByEmailAsync("joao@email.com", It.IsAny<CancellationToken>()))
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

        _userRepoMock.Setup(r => r.GetByEmailAsync("joao@email.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userManagerMock.Setup(m => m.CheckPasswordAsync(user, It.IsAny<string>()))
            .ReturnsAsync(false);

        var request = new LoginRequest
        {
            Email = "joao@email.com",
            Senha = "SenhaErrada"
        };

        Func<Task> act = () => _authService.LoginAsync(request);
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Email ou senha inválidos.");
    }

    [Fact]
    public async Task RefreshTokenAsync_WithValidToken_ShouldReturnNewToken()
    {
        var user = new User("João", "joao_123", "joao@email.com");
        var storedToken = new RefreshToken(user.Id, "token-hash", DateTime.UtcNow.AddDays(7));

        _tokenServiceMock.Setup(t => t.HashToken(It.IsAny<string>()))
            .Returns("token-hash");
        _refreshTokenRepoMock.Setup(r => r.GetByTokenHashAsync("token-hash", It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);
        _userRepoMock.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _tokenServiceMock.Setup(t => t.GenerateRefreshToken())
            .Returns(("new-refresh-token", "new-hash"));
        _tokenServiceMock.Setup(t => t.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<string>>()))
            .Returns("fake-jwt-token");
        _userManagerMock.Setup(m => m.GetRolesAsync(It.IsAny<User>()))
            .ReturnsAsync(["User"]);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

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
        _refreshTokenRepoMock.Setup(r => r.GetByTokenHashAsync("invalid-hash", It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        Func<Task> act = () => _authService.RefreshTokenAsync("invalid-token");
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Refresh token inválido ou expirado.");
    }

    [Fact]
    public async Task RefreshTokenAsync_WithExpiredToken_ShouldThrowException()
    {
        var storedToken = new RefreshToken(Guid.NewGuid(), "token-hash", DateTime.UtcNow.AddDays(-1));

        _tokenServiceMock.Setup(t => t.HashToken(It.IsAny<string>()))
            .Returns("token-hash");
        _refreshTokenRepoMock.Setup(r => r.GetByTokenHashAsync("token-hash", It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);

        Func<Task> act = () => _authService.RefreshTokenAsync("expired-token");
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Refresh token inválido ou expirado.");
    }

    [Fact]
    public async Task RefreshTokenAsync_WithRevokedToken_ShouldThrowException()
    {
        var storedToken = new RefreshToken(Guid.NewGuid(), "token-hash", DateTime.UtcNow.AddDays(7));
        storedToken.Revoke();

        _tokenServiceMock.Setup(t => t.HashToken(It.IsAny<string>()))
            .Returns("token-hash");
        _refreshTokenRepoMock.Setup(r => r.GetByTokenHashAsync("token-hash", It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);
        _refreshTokenRepoMock.Setup(r => r.GetActiveByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        Func<Task> act = () => _authService.RefreshTokenAsync("revoked-token");
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Refresh token inválido ou expirado.");
    }

    [Fact]
    public async Task RevokeRefreshTokensAsync_ShouldRevokeActiveTokens()
    {
        var userId = Guid.NewGuid();
        var token1 = new RefreshToken(userId, "hash1", DateTime.UtcNow.AddDays(7));
        var token2 = new RefreshToken(userId, "hash2", DateTime.UtcNow.AddDays(7));

        _refreshTokenRepoMock.Setup(r => r.GetActiveByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([token1, token2]);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(2);

        await _authService.RevokeRefreshTokensAsync(userId);

        token1.IsRevoked.Should().BeTrue();
        token2.IsRevoked.Should().BeTrue();
        _refreshTokenRepoMock.Verify(r => r.UpdateAsync(token1, It.IsAny<CancellationToken>()), Times.Once);
        _refreshTokenRepoMock.Verify(r => r.UpdateAsync(token2, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WithBothCpfAndRgEmpty_ShouldThrowException()
    {
        var request = new RegisterRequest
        {
            Nome = "João",
            UserName = "joao_123",
            Email = "joao@email.com",
            Senha = "SenhaForte123"
        };

        Func<Task> act = () => _authService.RegisterAsync(request);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("CPF ou RG deve ser informado.");
    }

    [Fact]
    public async Task RegisterAsync_WithExistingUserName_ShouldThrowException()
    {
        _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _userManagerMock.Setup(m => m.FindByNameAsync("joao_123"))
            .ReturnsAsync(new User("João", "joao_123", "joao@email.com"));

        var request = new RegisterRequest
        {
            Nome = "João",
            UserName = "joao_123",
            Email = "joao@email.com",
            Senha = "SenhaForte123",
            Cpf = "52998224725"
        };

        Func<Task> act = () => _authService.RegisterAsync(request);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("UserName já cadastrado.");
    }

    [Fact]
    public async Task RegisterAsync_WithCreateUserFailing_ShouldThrowException()
    {
        _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _userManagerMock.Setup(m => m.FindByNameAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);
        _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Senha muito curta" }));

        var request = new RegisterRequest
        {
            Nome = "João",
            UserName = "joao_123",
            Email = "joao@email.com",
            Senha = "Fraca1",
            Cpf = "52998224725"
        };

        Func<Task> act = () => _authService.RegisterAsync(request);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Falha ao criar usuário.");
    }

    [Fact]
    public async Task LoginAsync_WithLockedOutUser_ShouldThrowException()
    {
        var user = new User("João", "joao_123", "joao@email.com");

        _userRepoMock.Setup(r => r.GetByEmailAsync("joao@email.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userManagerMock.Setup(m => m.IsLockedOutAsync(user))
            .ReturnsAsync(true);

        var request = new LoginRequest
        {
            Email = "joao@email.com",
            Senha = "SenhaForte123"
        };

        Func<Task> act = () => _authService.LoginAsync(request);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Conta temporariamente bloqueada por muitas tentativas inválidas. Tente novamente em 15 minutos.");
    }

    [Fact]
    public async Task LoginAsync_WithEmailNotFound_ShouldThrowException()
    {
        _userRepoMock.Setup(r => r.GetByEmailAsync("naoexiste@email.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _userManagerMock.Setup(m => m.CheckPasswordAsync(It.IsAny<User>(), "fake"))
            .ReturnsAsync(false);

        var request = new LoginRequest
        {
            Email = "naoexiste@email.com",
            Senha = "SenhaForte123"
        };

        Func<Task> act = () => _authService.LoginAsync(request);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Email ou senha inválidos.");
    }

    [Fact]
    public async Task RefreshTokenAsync_WithUserNotFound_ShouldThrowException()
    {
        var storedToken = new RefreshToken(Guid.NewGuid(), "token-hash", DateTime.UtcNow.AddDays(7));

        _tokenServiceMock.Setup(t => t.HashToken(It.IsAny<string>()))
            .Returns("token-hash");
        _refreshTokenRepoMock.Setup(r => r.GetByTokenHashAsync("token-hash", It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);
        _userRepoMock.Setup(r => r.GetByIdAsync(storedToken.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        Func<Task> act = () => _authService.RefreshTokenAsync("valid-refresh-token");

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Usuário não encontrado.");
    }
}
