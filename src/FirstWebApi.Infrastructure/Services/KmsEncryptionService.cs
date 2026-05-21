using System.Collections.Concurrent;
using System.Security.Cryptography;
using Amazon;
using Amazon.KeyManagementService;
using Amazon.KeyManagementService.Model;
using FirstWebApi.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FirstWebApi.Infrastructure.Services;

public class KmsEncryptionService : IEncryptionService
{
    private readonly IAmazonKeyManagementService _kmsClient;
    private readonly string _keyId;
    private readonly ILogger<KmsEncryptionService> _logger;
    private static readonly ConcurrentDictionary<string, byte[]> DataKeyCache = new();
    private static readonly TimeSpan CacheExpiry = TimeSpan.FromMinutes(15);
    private static DateTime _lastCacheCleanup = DateTime.UtcNow;

    public KmsEncryptionService(IConfiguration configuration, ILogger<KmsEncryptionService> logger)
    {
        _logger = logger;
        var kmsConfig = configuration.GetSection("Kms");
        _keyId = kmsConfig["KeyId"] ?? throw new InvalidOperationException("KMS KeyId nao configurado.");

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

        InitializeKeyAsync().GetAwaiter().GetResult();
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
                    Description = "Chave mestra para criptografia de dados sensiveis LGPD",
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
                _logger.LogInformation("Chave KMS ja existe: {KeyId}", _keyId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Nao foi possivel inicializar a chave KMS. Verifique se o LocalStack esta rodando.");
        }
    }

    public async Task<(byte[] ciphertext, byte[] iv, byte[] tag, byte[] encryptedDataKey)> EncryptAsync(string plaintext)
    {
        var dataKeyResponse = await _kmsClient.GenerateDataKeyAsync(new GenerateDataKeyRequest
        {
            KeyId = _keyId,
            KeySpec = DataKeySpec.AES_256
        });

        var plaintextKeyBytes = dataKeyResponse.Plaintext.ToArray();
        var encryptedDataKeyBytes = dataKeyResponse.CiphertextBlob.ToArray();

        var cacheKey = Convert.ToBase64String(encryptedDataKeyBytes);
        DataKeyCache[cacheKey] = plaintextKeyBytes;

        var plaintextBytes = System.Text.Encoding.UTF8.GetBytes(plaintext);
        using var aes = Aes.Create();
        aes.KeySize = 256;
        aes.Key = plaintextKeyBytes;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var ciphertextBytes = encryptor.TransformFinalBlock(plaintextBytes, 0, plaintextBytes.Length);

        var iv = aes.IV;
        var tag = Array.Empty<byte>();

        CleanCache();

        return (ciphertextBytes, iv, tag, encryptedDataKeyBytes);
    }

    public async Task<string> DecryptAsync(byte[] ciphertext, byte[] iv, byte[] tag, byte[] encryptedDataKey)
    {
        var cacheKey = Convert.ToBase64String(encryptedDataKey);

        if (!DataKeyCache.TryGetValue(cacheKey, out var plaintextKey))
        {
            var decryptResponse = await _kmsClient.DecryptAsync(new DecryptRequest
            {
                CiphertextBlob = new MemoryStream(encryptedDataKey),
                KeyId = _keyId
            });
            plaintextKey = decryptResponse.Plaintext.ToArray();
            DataKeyCache[cacheKey] = plaintextKey;
        }

        using var aes = Aes.Create();
        aes.KeySize = 256;
        aes.Key = plaintextKey;
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        var plaintextBytes = decryptor.TransformFinalBlock(ciphertext, 0, ciphertext.Length);

        CleanCache();

        return System.Text.Encoding.UTF8.GetString(plaintextBytes);
    }

    private void CleanCache()
    {
        if (DateTime.UtcNow - _lastCacheCleanup > CacheExpiry)
        {
            _lastCacheCleanup = DateTime.UtcNow;
            DataKeyCache.Clear();
        }
    }
}
