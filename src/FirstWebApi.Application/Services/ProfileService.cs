using FirstWebApi.Application.DTOs;
using FirstWebApi.Application.DTOs.Response;
using FirstWebApi.Application.Exceptions;
using FirstWebApi.Application.Interfaces;
using FirstWebApi.Domain;
using FirstWebApi.Domain.Entities;
using FirstWebApi.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace FirstWebApi.Application.Services;

public class ProfileService(
    IUserRepository userRepository,
    ISensitiveDataService sensitiveData,
    UserManager<User> userManager) : IProfileService
{

    public async Task<UserResponse> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            throw new KeyNotFoundException("Usuário não encontrado.");

        var roles = await userManager.GetRolesAsync(user);
        var (cpf, rg, endereco) = await sensitiveData.DecryptUserDataAsync(user, userId, cancellationToken);

        var response = new UserResponse
        {
            Id = user.Id,
            Nome = user.Nome,
            UserName = user.UserName!,
            Email = user.Email!,
            Cpf = cpf,
            Rg = rg,
            Endereco = endereco,
            Role = roles.FirstOrDefault() ?? Roles.User,
            CreatedAt = user.CreatedAt,
            HasFullData = true
        };

        ApplyMask(response);
        return response;
    }

    public async Task<UserResponse> GetFullProfileAsync(Guid userId, string senha, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            throw new KeyNotFoundException("Usuário não encontrado.");

        var passwordValid = await userManager.CheckPasswordAsync(user, senha);
        if (!passwordValid)
            throw new BadRequestException("Senha inválida. Reautenticação necessária.");

        var roles = await userManager.GetRolesAsync(user);
        var (cpf, rg, endereco) = await sensitiveData.DecryptUserDataAsync(user, userId, cancellationToken);

        return new UserResponse
        {
            Id = user.Id,
            Nome = user.Nome,
            UserName = user.UserName!,
            Email = user.Email!,
            Cpf = cpf,
            Rg = rg,
            Endereco = endereco,
            Role = roles.FirstOrDefault() ?? Roles.User,
            CreatedAt = user.CreatedAt,
            HasFullData = true
        };
    }

    private static string? MaskCpf(string? cpf)
    {
        if (string.IsNullOrEmpty(cpf) || cpf.Length < 11)
            return cpf;

        return $"***.{cpf[3..6]}.{cpf[6..9]}-**";
    }

    private static string? MaskRg(string? rg)
    {
        if (string.IsNullOrEmpty(rg) || rg.Length < 4)
            return rg;

        return $"*****-{rg[^4..]}";
    }

    private static EnderecoInfo MaskEndereco(EnderecoInfo endereco) => new()
    {
        Cidade = endereco.Cidade,
        Estado = endereco.Estado
    };

    private static void ApplyMask(UserResponse response)
    {
        response.Cpf = MaskCpf(response.Cpf);
        response.Rg = MaskRg(response.Rg);
        response.Endereco = response.Endereco is not null ? MaskEndereco(response.Endereco) : null;
        response.HasFullData = false;
    }
}
