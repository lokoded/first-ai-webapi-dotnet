using System.Text.Json;
using FirstWebApi.Application.DTOs;
using FirstWebApi.Application.Interfaces;
using FirstWebApi.Application.Services;
using FirstWebApi.Domain.Entities;
using FirstWebApi.Domain.Interfaces;
using FirstWebApi.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace FirstWebApi.UnitTests.Application;

public class SensitiveDataServiceTests
{
    private readonly Mock<IEncryptionService> _encryptionMock;
    private readonly Mock<IAddressRepository> _addressRepoMock;
    private readonly Mock<ILogger<SensitiveDataService>> _loggerMock;
    private readonly SensitiveDataService _service;
    private readonly DadoProtegido _fakeEncrypted = new([1, 2, 3, 4]);

    public SensitiveDataServiceTests()
    {
        _encryptionMock = new Mock<IEncryptionService>();
        _addressRepoMock = new Mock<IAddressRepository>();
        _loggerMock = new Mock<ILogger<SensitiveDataService>>();

        _encryptionMock
            .Setup(e => e.EncryptAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_fakeEncrypted);

        _service = new SensitiveDataService(
            _encryptionMock.Object,
            _addressRepoMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task EncryptCpfAsync_WithValidCpf_ShouldEncryptAndSet()
    {
        var user = new User("João", "joao", "joao@email.com");

        await _service.EncryptCpfAsync(user, "529.982.247-25");

        user.CpfData.Should().Be(_fakeEncrypted);
        _encryptionMock.Verify(e => e.EncryptAsync("52998224725", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EncryptCpfAsync_WithNullCpf_ShouldDoNothing()
    {
        var user = new User("João", "joao", "joao@email.com");

        await _service.EncryptCpfAsync(user, null);

        user.CpfData.Should().BeNull();
        _encryptionMock.Verify(e => e.EncryptAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EncryptCpfAsync_WithEmptyCpf_ShouldDoNothing()
    {
        var user = new User("João", "joao", "joao@email.com");

        await _service.EncryptCpfAsync(user, "");

        user.CpfData.Should().BeNull();
        _encryptionMock.Verify(e => e.EncryptAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EncryptRgAsync_WithValidRg_ShouldEncryptAndSet()
    {
        var user = new User("João", "joao", "joao@email.com");

        await _service.EncryptRgAsync(user, "12.345.678-9");

        user.RgData.Should().Be(_fakeEncrypted);
        _encryptionMock.Verify(e => e.EncryptAsync("12.345.678-9", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EncryptRgAsync_WithNullRg_ShouldDoNothing()
    {
        var user = new User("João", "joao", "joao@email.com");

        await _service.EncryptRgAsync(user, null);

        user.RgData.Should().BeNull();
        _encryptionMock.Verify(e => e.EncryptAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EncryptEnderecoAsync_WithValidEndereco_ShouldEncryptAndSave()
    {
        var userId = Guid.NewGuid();
        var endereco = new EnderecoInfo
        {
            Logradouro = "Rua ABC",
            Numero = "123",
            Bairro = "Centro",
            Cidade = "São Paulo",
            Estado = "SP",
            Cep = "01001-000",
            Pais = "Brasil"
        };

        _addressRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Address>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _service.EncryptEnderecoAsync(userId, endereco);

        result.Should().NotBeNull();
        result!.UserId.Should().Be(userId);
        result.Data.Should().Be(_fakeEncrypted);
        _addressRepoMock.Verify(r => r.AddAsync(It.Is<Address>(a => a.UserId == userId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EncryptEnderecoAsync_WithNullEndereco_ShouldReturnNull()
    {
        var result = await _service.EncryptEnderecoAsync(Guid.NewGuid(), null);

        result.Should().BeNull();
        _addressRepoMock.Verify(r => r.AddAsync(It.IsAny<Address>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EncryptEnderecoAsync_WithEmptyEndereco_ShouldReturnNull()
    {
        var endereco = new EnderecoInfo();

        var result = await _service.EncryptEnderecoAsync(Guid.NewGuid(), endereco);

        result.Should().BeNull();
        _addressRepoMock.Verify(r => r.AddAsync(It.IsAny<Address>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DecryptUserDataAsync_WithAllData_ShouldDecryptAll()
    {
        var userId = Guid.NewGuid();
        var user = new User("João", "joao", "joao@email.com");
        user.SetCpfData(_fakeEncrypted);
        user.SetRgData(_fakeEncrypted);

        var address = new Address(userId);
        address.SetEncryptedData(_fakeEncrypted);

        var enderecoJson = """{"Logradouro":"Rua ABC","Numero":"123","Bairro":"Centro","Cidade":"São Paulo","Estado":"SP","Cep":"01001-000","Pais":"Brasil"}""";

        _encryptionMock
            .SetupSequence(e => e.DecryptAsync(_fakeEncrypted, It.IsAny<CancellationToken>()))
            .ReturnsAsync("52998224725")
            .ReturnsAsync("123456789")
            .ReturnsAsync(enderecoJson);

        _addressRepoMock
            .Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(address);

        var (cpf, rg, endereco) = await _service.DecryptUserDataAsync(user, userId);

        cpf.Should().Be("52998224725");
        rg.Should().Be("123456789");
        endereco.Should().NotBeNull();
        endereco!.Logradouro.Should().Be("Rua ABC");
    }

    [Fact]
    public async Task DecryptUserDataAsync_WithDecryptFailure_ShouldFallbackGracefully()
    {
        var userId = Guid.NewGuid();
        var user = new User("João", "joao", "joao@email.com");
        user.SetCpfData(_fakeEncrypted);
        user.SetRgData(_fakeEncrypted);

        _encryptionMock
            .Setup(e => e.DecryptAsync(_fakeEncrypted, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Falha na descriptografia"));

        _addressRepoMock
            .Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Address?)null);

        var (cpf, rg, endereco) = await _service.DecryptUserDataAsync(user, userId);

        cpf.Should().BeNull();
        rg.Should().BeNull();
        endereco.Should().BeNull();
    }

    [Fact]
    public async Task DecryptUserDataAsync_WithNoAddress_ShouldReturnNullEndereco()
    {
        var userId = Guid.NewGuid();
        var user = new User("João", "joao", "joao@email.com");

        _addressRepoMock
            .Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Address?)null);

        _encryptionMock
            .Setup(e => e.DecryptAsync(It.IsAny<DadoProtegido>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("value");

        var (_, _, endereco) = await _service.DecryptUserDataAsync(user, userId);

        endereco.Should().BeNull();
    }
}
