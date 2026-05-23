using System.Text.Json;
using FirstWebApi.Application.DTOs;
using FirstWebApi.Application.Interfaces;
using FirstWebApi.Domain.Entities;
using FirstWebApi.Domain.Interfaces;
using FirstWebApi.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace FirstWebApi.Application.Services;

public class SensitiveDataService(
    IEncryptionService encryptionService,
    IAddressRepository addressRepository,
    ILogger<SensitiveDataService> logger) : ISensitiveDataService
{
    public async Task EncryptCpfAsync(User user, string? cpf)
    {
        if (string.IsNullOrEmpty(cpf))
            return;

        var cpfVo = new Cpf(cpf);
        var data = await encryptionService.EncryptAsync(cpfVo.Numero);
        user.SetCpfData(data);
    }

    public async Task EncryptRgAsync(User user, string? rg)
    {
        if (string.IsNullOrEmpty(rg))
            return;

        var data = await encryptionService.EncryptAsync(rg);
        user.SetRgData(data);
    }

    public async Task<Address?> EncryptEnderecoAsync(Guid userId, EnderecoInfo? endereco)
    {
        if (endereco is null || endereco.IsEmpty)
            return null;

        var json = JsonSerializer.Serialize(endereco);
        var data = await encryptionService.EncryptAsync(json);
        var address = new Address(userId);
        address.SetEncryptedData(data);
        await addressRepository.AddAsync(address);
        return address;
    }

    public async Task<(string? cpf, string? rg, EnderecoInfo? endereco)> DecryptUserDataAsync(User user, Guid userId)
    {
        var cpf = await DecryptFieldAsync(user.CpfData, "CPF", userId);
        var rg = await DecryptFieldAsync(user.RgData, "RG", userId);

        var address = await addressRepository.GetByUserIdAsync(userId);
        var enderecoJson = await DecryptFieldAsync(address?.Data, "endereço", userId);

        EnderecoInfo? endereco = null;
        if (enderecoJson is not null)
            endereco = JsonSerializer.Deserialize<EnderecoInfo>(enderecoJson);

        return (cpf, rg, endereco);
    }

    private async Task<string?> DecryptFieldAsync(DadoProtegido? data, string fieldName, Guid userId)
    {
        if (data is not DadoProtegido encrypted)
            return null;

        try
        {
            return await encryptionService.DecryptAsync(encrypted);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao descriptografar {FieldName} do usuário {UserId}", fieldName, userId);
            return null;
        }
    }
}
