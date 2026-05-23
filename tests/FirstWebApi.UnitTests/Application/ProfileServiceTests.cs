using FirstWebApi.Application.DTOs;
using FirstWebApi.Application.Exceptions;
using FirstWebApi.Application.Interfaces;
using FirstWebApi.Application.Services;
using FirstWebApi.Domain.Entities;
using FirstWebApi.Domain.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace FirstWebApi.UnitTests.Application;

public class ProfileServiceTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<ISensitiveDataService> _sensitiveDataMock;
    private readonly ProfileService _profileService;

    public ProfileServiceTests()
    {
        _userManagerMock = new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(), null!, null!, null!, null!, null!, null!, null!, null!);
        _userRepoMock = new Mock<IUserRepository>();
        _sensitiveDataMock = new Mock<ISensitiveDataService>();

        _profileService = new ProfileService(
            _userRepoMock.Object,
            _sensitiveDataMock.Object,
            _userManagerMock.Object);
    }

    [Fact]
    public async Task GetProfileAsync_WithExistingUser_ShouldReturnProfile()
    {
        var userId = Guid.NewGuid();
        var user = new User("João", "joao_123", "joao@email.com");

        _userRepoMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userManagerMock.Setup(m => m.GetRolesAsync(user))
            .ReturnsAsync(["User"]);
        _sensitiveDataMock.Setup(m => m.DecryptUserDataAsync(user, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(((string?)null, (string?)null, (EnderecoInfo?)null));

        var result = await _profileService.GetProfileAsync(userId);

        result.Should().NotBeNull();
        result.Nome.Should().Be("João");
        result.Email.Should().Be("joao@email.com");
    }

    [Fact]
    public async Task GetProfileAsync_WithNonExistentUser_ShouldThrowException()
    {
        _userRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        Func<Task> act = () => _profileService.GetProfileAsync(Guid.NewGuid());
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Usuário não encontrado.");
    }

    [Fact]
    public async Task GetFullProfileAsync_WithValidPassword_ShouldReturnFullProfile()
    {
        var userId = Guid.NewGuid();
        var user = new User("João", "joao_123", "joao@email.com");

        _userRepoMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userManagerMock.Setup(m => m.CheckPasswordAsync(user, "SenhaForte123"))
            .ReturnsAsync(true);
        _userManagerMock.Setup(m => m.GetRolesAsync(user))
            .ReturnsAsync(["User"]);
        _sensitiveDataMock.Setup(m => m.DecryptUserDataAsync(user, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(("52998224725", null, (EnderecoInfo?)null));

        var result = await _profileService.GetFullProfileAsync(userId, "SenhaForte123");

        result.Should().NotBeNull();
        result.Nome.Should().Be("João");
        result.Cpf.Should().Be("52998224725");
        result.HasFullData.Should().BeTrue();
    }

    [Fact]
    public async Task GetFullProfileAsync_WithInvalidPassword_ShouldThrowException()
    {
        var userId = Guid.NewGuid();
        var user = new User("João", "joao_123", "joao@email.com");

        _userRepoMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userManagerMock.Setup(m => m.CheckPasswordAsync(user, "SenhaErrada"))
            .ReturnsAsync(false);

        Func<Task> act = () => _profileService.GetFullProfileAsync(userId, "SenhaErrada");

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Senha inválida. Reautenticação necessária.");
    }

    [Fact]
    public async Task GetFullProfileAsync_WithNonExistentUser_ShouldThrowException()
    {
        _userRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        Func<Task> act = () => _profileService.GetFullProfileAsync(Guid.NewGuid(), "SenhaForte123");

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Usuário não encontrado.");
    }
}
