using FirstWebApi.Application.DTOs.Response;
using FirstWebApi.Application.Exceptions;
using FirstWebApi.Application.Interfaces;
using FirstWebApi.Domain.Entities;
using FirstWebApi.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace FirstWebApi.Application.Services;

public class ProfileService(
    IUserRepository userRepository,
    ISensitiveDataService sensitiveData,
    UserManager<User> userManager) : IProfileService
{

    public async Task<UserResponse> GetProfileAsync(Guid userId)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("Usuário não encontrado.");

        var roles = await userManager.GetRolesAsync(user);
        var (cpf, rg, endereco) = await sensitiveData.DecryptUserDataAsync(user, userId);

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
        var (cpf, rg, endereco) = await sensitiveData.DecryptUserDataAsync(user, userId);

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
}
