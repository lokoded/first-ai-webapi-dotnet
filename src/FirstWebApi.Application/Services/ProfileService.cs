using System.Text.Json;
using FirstWebApi.Application.DTOs;
using FirstWebApi.Application.DTOs.Response;
using FirstWebApi.Application.Exceptions;
using FirstWebApi.Application.Interfaces;
using FirstWebApi.Domain.Entities;
using FirstWebApi.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace FirstWebApi.Application.Services;

public class ProfileService(
    IUserRepository userRepository,
    IAddressRepository addressRepository,
    IEncryptionService encryptionService,
    UserManager<User> userManager,
    ILogger<ProfileService> logger) : IProfileService
{

    public async Task<UserResponse> GetProfileAsync(Guid userId)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("Usuário não encontrado.");

        var roles = await userManager.GetRolesAsync(user);
        var (cpf, rg, endereco) = await DecryptUserDataAsync(user, userId);

        return new UserResponse
        {
            Id = user.Id,
            Nome = user.Nome,
            UserName = user.UserName!,
            Email = user.Email!,
            Cpf = cpf,
            Rg = rg,
            Endereco = endereco,
            Role = roles.FirstOrDefault() ?? "User",
            CreatedAt = user.CreatedAt,
            HasFullData = true
        };
    }

    public async Task<UserResponse> GetFullProfileAsync(Guid userId, string senha)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("Usuário não encontrado.");

        var passwordValid = await userManager.CheckPasswordAsync(user, senha);
        if (!passwordValid)
            throw new BadRequestException("Senha inválida. Reautenticação necessária.");

        var roles = await userManager.GetRolesAsync(user);
        var (cpf, rg, endereco) = await DecryptUserDataAsync(user, userId);

        return new UserResponse
        {
            Id = user.Id,
            Nome = user.Nome,
            UserName = user.UserName!,
            Email = user.Email!,
            Cpf = cpf,
            Rg = rg,
            Endereco = endereco,
            Role = roles.FirstOrDefault() ?? "User",
            CreatedAt = user.CreatedAt,
            HasFullData = true
        };
    }

    private async Task<(string? cpf, string? rg, EnderecoInfo? endereco)> DecryptUserDataAsync(User user, Guid userId)
    {
        var cpf = await DecryptFieldAsync(
            user.CpfCiphertext, user.CpfIv, user.CpfTag, user.CpfEncryptedDataKey,
            "CPF", userId);

        var rg = await DecryptFieldAsync(
            user.RgCiphertext, user.RgIv, user.RgTag, user.RgEncryptedDataKey,
            "RG", userId);

        var address = await addressRepository.GetByUserIdAsync(userId);
        var enderecoJson = await DecryptFieldAsync(
            address?.Ciphertext, address?.Iv, address?.Tag, address?.EncryptedDataKey,
            "endereço", userId);

        EnderecoInfo? endereco = null;
        if (enderecoJson is not null)
            endereco = JsonSerializer.Deserialize<EnderecoInfo>(enderecoJson);

        return (cpf, rg, endereco);
    }

    private async Task<string?> DecryptFieldAsync(
        byte[]? ciphertext, byte[]? iv, byte[]? tag, byte[]? encryptedDataKey,
        string fieldName, Guid userId)
    {
        if (ciphertext is null || iv is null || tag is null || encryptedDataKey is null)
            return null;

        try
        {
            return await encryptionService.DecryptAsync(ciphertext, iv, tag, encryptedDataKey);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao descriptografar {FieldName} do usuário {UserId}", fieldName, userId);
            return null;
        }
    }

}
