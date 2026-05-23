using System.Text;
using Amazon.KeyManagementService;
using Amazon.KeyManagementService.Model;
using FirstWebApi.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace FirstWebApi.UnitTests.Infrastructure;

public class KmsEncryptionServiceTests : IDisposable
{
    private readonly Mock<IAmazonKeyManagementService> _kmsMock;
    private readonly MemoryCache _cache;
    private readonly KmsEncryptionService _service;

    public KmsEncryptionServiceTests()
    {
        _kmsMock = new Mock<IAmazonKeyManagementService>();
        _cache = new MemoryCache(new MemoryCacheOptions());

        var configData = new Dictionary<string, string?>
        {
            { "Kms:KeyId", "alias/lgpd-test-key" },
            { "Kms:Endpoint", "http://localhost:4566" },
            { "Kms:Region", "us-east-1" },
            { "Kms:UseHttp", "true" },
            { "Kms:AccessKey", "fake-access-key" },
            { "Kms:SecretKey", "fake-secret-key" }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        var loggerMock = new Mock<ILogger<KmsEncryptionService>>();

        _service = new KmsEncryptionService(
            configuration,
            loggerMock.Object,
            _cache,
            _kmsMock.Object);
    }

    public void Dispose() => _cache.Dispose();

    [Fact]
    public async Task EncryptAsync_ShouldReturnProtectedData()
    {
        var plaintext = "52998224725";
        var plaintextKey = new byte[32];
        Random.Shared.NextBytes(plaintextKey);
        var encryptedDataKey = Encoding.UTF8.GetBytes("encrypted-data-key-blob");

        _kmsMock
            .Setup(k => k.ListAliasesAsync(It.IsAny<ListAliasesRequest>(), default))
            .ReturnsAsync(new ListAliasesResponse
            {
                Aliases = [new AliasListEntry { AliasName = "alias/lgpd-test-key" }]
            });

        _kmsMock
            .Setup(k => k.GenerateDataKeyAsync(It.IsAny<GenerateDataKeyRequest>(), default))
            .ReturnsAsync(new GenerateDataKeyResponse
            {
                Plaintext = new MemoryStream(plaintextKey),
                CiphertextBlob = new MemoryStream(encryptedDataKey)
            });

        var result = await _service.EncryptAsync(plaintext);

        result.Valor.Should().NotBeNull();
        result.Valor.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task DecryptAsync_ShouldReturnOriginalPlaintext()
    {
        var plaintext = "52998224725";
        var plaintextKey = new byte[32];
        Random.Shared.NextBytes(plaintextKey);
        var encryptedDataKey = Encoding.UTF8.GetBytes("encrypted-data-key-blob");

        _kmsMock
            .Setup(k => k.ListAliasesAsync(It.IsAny<ListAliasesRequest>(), default))
            .ReturnsAsync(new ListAliasesResponse
            {
                Aliases = [new AliasListEntry { AliasName = "alias/lgpd-test-key" }]
            });

        _kmsMock
            .Setup(k => k.GenerateDataKeyAsync(It.IsAny<GenerateDataKeyRequest>(), default))
            .ReturnsAsync(new GenerateDataKeyResponse
            {
                Plaintext = new MemoryStream(plaintextKey),
                CiphertextBlob = new MemoryStream(encryptedDataKey)
            });

        _kmsMock
            .Setup(k => k.DecryptAsync(It.IsAny<DecryptRequest>(), default))
            .ReturnsAsync(new DecryptResponse
            {
                Plaintext = new MemoryStream(plaintextKey)
            });

        var encryptedData = await _service.EncryptAsync(plaintext);
        var decrypted = await _service.DecryptAsync(encryptedData);

        decrypted.Should().Be(plaintext);
    }
}
