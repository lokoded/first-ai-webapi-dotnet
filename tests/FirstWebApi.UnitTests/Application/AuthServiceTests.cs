using FirstWebApi.Application.DTOs.Request;
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
            Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);
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
    public async Task RegisterAsync_ComDadosValidos_DeveRetornarToken()
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
    public async Task RegisterAsync_ComEmailExistente_DeveLancarExcecao()
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
    public async Task LoginAsync_ComCredenciaisValidas_DeveRetornarToken()
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
    public async Task LoginAsync_ComSenhaInvalida_DeveLancarExcecao()
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
}
