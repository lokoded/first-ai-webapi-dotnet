using System.Security.Cryptography;
using Amazon;
using Amazon.KeyManagementService;
using Amazon.KeyManagementService.Model;
using FirstWebApi.Application.Interfaces;
using FirstWebApi.Domain.ValueObjects;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FirstWebApi.Infrastructure.Services;

public class KmsEncryptionService(
    IConfiguration configuration,
    ILogger<KmsEncryptionService> logger,
    IMemoryCache memoryCache,
    IAmazonKeyManagementService? kmsClient = null) : IEncryptionService
{
    private readonly IAmazonKeyManagementService _kmsClient = kmsClient ?? CreateKmsClient(configuration);
    private readonly string _keyId = configuration.GetSection("Kms")["KeyId"]
        ?? throw new InvalidOperationException("KMS KeyId não configurado.");
    private readonly ILogger<KmsEncryptionService> _logger = logger;
    private readonly IMemoryCache _cache = memoryCache;
    private static readonly TimeSpan CacheSlidingExpiry = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan CacheAbsoluteExpiry = TimeSpan.FromHours(1);
    private Lazy<Task>? _initialization;

    private static IAmazonKeyManagementService CreateKmsClient(IConfiguration configuration)
    {
        var kmsConfig = configuration.GetSection("Kms");
        var endpoint = kmsConfig["Endpoint"];
        var useHttp = kmsConfig.GetValue<bool>("UseHttp");
        var region = kmsConfig["Region"] ?? "us-east-1";
        var accessKey = kmsConfig["AccessKey"];
        var secretKey = kmsConfig["SecretKey"];

        var config = new AmazonKeyManagementServiceConfig
        {
            RegionEndpoint = RegionEndpoint.GetBySystemName(region)
        };

        if (!string.IsNullOrEmpty(endpoint))
        {
            config.ServiceURL = endpoint;
            config.UseHttp = useHttp;
            config.AuthenticationRegion = region;
        }

        if (!string.IsNullOrEmpty(accessKey) && !string.IsNullOrEmpty(secretKey))
            return new AmazonKeyManagementServiceClient(accessKey, secretKey, config);

        return new AmazonKeyManagementServiceClient(config);
    }

    private async Task EnsureInitializedAsync()
    {
        _initialization ??= new Lazy<Task>(() => InitializeKeyAsync());
        await _initialization.Value;
    }

    private async Task InitializeKeyAsync()
    {
        try
        {
            var aliasName = _keyId.Contains('/') ? _keyId : $"alias/{_keyId}";
            var listAliasesResponse = await _kmsClient.ListAliasesAsync(new ListAliasesRequest { Limit = 100 });
            var keyExists = listAliasesResponse.Aliases.Any(a => a.AliasName == aliasName);

            if (!keyExists)
            {
                _logger.LogInformation("Criando nova chave KMS: {KeyId}", _keyId);
                var createResponse = await _kmsClient.CreateKeyAsync(new CreateKeyRequest
                {
                    Description = "Chave mestra para criptografia de dados sensíveis LGPD",
                    KeyUsage = KeyUsageType.ENCRYPT_DECRYPT,
                    KeySpec = KeySpec.SYMMETRIC_DEFAULT
                });

                var newKeyId = createResponse.KeyMetadata.KeyId;
                await _kmsClient.CreateAliasAsync(new CreateAliasRequest
                {
                    AliasName = aliasName,
                    TargetKeyId = newKeyId
                });

                _logger.LogInformation("Chave KMS criada: {KeyId}", newKeyId);
            }
            else
            {
                _logger.LogInformation("Chave KMS já existe: {KeyId}", _keyId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Não foi possível inicializar a chave KMS. Verifique se o LocalStack está rodando.");
        }
    }

    public async Task<EncryptedData> EncryptAsync(string plaintext)
    {
        await EnsureInitializedAsync();
        var dataKeyResponse = await _kmsClient.GenerateDataKeyAsync(new GenerateDataKeyRequest
        {
            KeyId = _keyId,
            KeySpec = DataKeySpec.AES_256
        });

        var plaintextKeyBytes = dataKeyResponse.Plaintext.ToArray();
        var encryptedDataKeyBytes = dataKeyResponse.CiphertextBlob.ToArray();

        var cacheKey = Convert.ToBase64String(encryptedDataKeyBytes);
        _cache.Set(cacheKey, plaintextKeyBytes, new MemoryCacheEntryOptions
        {
            SlidingExpiration = CacheSlidingExpiry,
            AbsoluteExpirationRelativeToNow = CacheAbsoluteExpiry
        });

        var plaintextBytes = System.Text.Encoding.UTF8.GetBytes(plaintext);
        var nonce = new byte[AesGcm.NonceByteSizes.MaxSize];
        RandomNumberGenerator.Fill(nonce);

        var ciphertextBytes = new byte[plaintextBytes.Length];
        var tag = new byte[AesGcm.TagByteSizes.MaxSize];

        using var aesGcm = new AesGcm(plaintextKeyBytes, AesGcm.TagByteSizes.MaxSize);
        aesGcm.Encrypt(nonce, plaintextBytes, ciphertextBytes, tag);

        return new EncryptedData(ciphertextBytes, nonce, tag, encryptedDataKeyBytes);
    }

    public async Task<string> DecryptAsync(EncryptedData data)
    {
        await EnsureInitializedAsync();
        var cacheKey = Convert.ToBase64String(data.EncryptedDataKey);

        if (!_cache.TryGetValue(cacheKey, out byte[]? plaintextKey))
        {
            var decryptResponse = await _kmsClient.DecryptAsync(new DecryptRequest
            {
                CiphertextBlob = new MemoryStream(data.EncryptedDataKey),
                KeyId = _keyId
            });
            plaintextKey = decryptResponse.Plaintext.ToArray();
            _cache.Set(cacheKey, plaintextKey, new MemoryCacheEntryOptions
            {
                SlidingExpiration = CacheSlidingExpiry,
                AbsoluteExpirationRelativeToNow = CacheAbsoluteExpiry
            });
        }

        var plaintextBytes = new byte[data.Ciphertext.Length];
        using var aesGcm = new AesGcm(plaintextKey!, AesGcm.TagByteSizes.MaxSize);
        aesGcm.Decrypt(data.Iv, data.Ciphertext, data.Tag, plaintextBytes);

        return System.Text.Encoding.UTF8.GetString(plaintextBytes);
    }
}
