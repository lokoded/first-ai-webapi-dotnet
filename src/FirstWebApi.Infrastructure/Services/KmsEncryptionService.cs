using System.Security.Cryptography;
using Amazon;
using Amazon.KeyManagementService;
using Amazon.KeyManagementService.Model;
using FirstWebApi.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FirstWebApi.Infrastructure.Services;

public class KmsEncryptionService : IEncryptionService
{
    private readonly IAmazonKeyManagementService _kmsClient;
    private readonly string _keyId;
    private readonly ILogger<KmsEncryptionService> _logger;
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan CacheSlidingExpiry = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan CacheAbsoluteExpiry = TimeSpan.FromHours(1);
    private readonly Lazy<Task> _initialization;

    public KmsEncryptionService(IConfiguration configuration, ILogger<KmsEncryptionService> logger, IMemoryCache memoryCache)
    {
        _logger = logger;
        _cache = memoryCache;
        var kmsConfig = configuration.GetSection("Kms");
        _keyId = kmsConfig["KeyId"] ?? throw new InvalidOperationException("KMS KeyId não configurado.");

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
        {
            _kmsClient = new AmazonKeyManagementServiceClient(accessKey, secretKey, config);
        }
        else
        {
            _kmsClient = new AmazonKeyManagementServiceClient(config);
        }

        _initialization = new Lazy<Task>(InitializeKeyAsync);
    }

    private async Task EnsureInitializedAsync()
    {
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

    public async Task<(byte[] ciphertext, byte[] iv, byte[] tag, byte[] encryptedDataKey)> EncryptAsync(string plaintext)
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
        var nonce = new byte[AesGcm.NonceByteSizes.MaxSize]; // 12 bytes
        RandomNumberGenerator.Fill(nonce);

        var ciphertextBytes = new byte[plaintextBytes.Length];
        var tag = new byte[AesGcm.TagByteSizes.MaxSize]; // 16 bytes

        using var aesGcm = new AesGcm(plaintextKeyBytes);
        aesGcm.Encrypt(nonce, plaintextBytes, ciphertextBytes, tag);

        return (ciphertextBytes, nonce, tag, encryptedDataKeyBytes);
    }

    public async Task<string> DecryptAsync(byte[] ciphertext, byte[] iv, byte[] tag, byte[] encryptedDataKey)
    {
        await EnsureInitializedAsync();
        var cacheKey = Convert.ToBase64String(encryptedDataKey);

        if (!_cache.TryGetValue(cacheKey, out byte[]? plaintextKey))
        {
            var decryptResponse = await _kmsClient.DecryptAsync(new DecryptRequest
            {
                CiphertextBlob = new MemoryStream(encryptedDataKey),
                KeyId = _keyId
            });
            plaintextKey = decryptResponse.Plaintext.ToArray();
            _cache.Set(cacheKey, plaintextKey, new MemoryCacheEntryOptions
            {
                SlidingExpiration = CacheSlidingExpiry,
                AbsoluteExpirationRelativeToNow = CacheAbsoluteExpiry
            });
        }

        var plaintextBytes = new byte[ciphertext.Length];
        using var aesGcm = new AesGcm(plaintextKey);
        aesGcm.Decrypt(iv, ciphertext, tag, plaintextBytes);

        return System.Text.Encoding.UTF8.GetString(plaintextBytes);
    }
}
