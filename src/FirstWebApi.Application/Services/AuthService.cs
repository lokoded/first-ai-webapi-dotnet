using System.Text.Json;
using FirstWebApi.Application.DTOs.Request;
using FirstWebApi.Application.DTOs.Response;
using FirstWebApi.Application.Exceptions;
using FirstWebApi.Application.Interfaces;
using FirstWebApi.Domain.Entities;
using FirstWebApi.Domain.Interfaces;
using FirstWebApi.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace FirstWebApi.Application.Services;

public class AuthService(
    UserManager<User> userManager,
    IUserRepository userRepository,
    ITokenService tokenService,
    IEncryptionService encryptionService,
    IUnitOfWork unitOfWork,
    ILogger<AuthService> logger,
    IAddressRepository addressRepository,
    IRefreshTokenRepository refreshTokenRepository) : IAuthService
{
    private static readonly TimeSpan RefreshTokenExpiry = TimeSpan.FromDays(7);
    private const string InvalidEmailOrPassword = "Email ou senha inválidos.";
    private const string InvalidRefreshToken = "Refresh token inválido ou expirado.";
    private const string UserNotFound = "Usuário não encontrado.";

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var existingEmail = await userRepository.GetByEmailAsync(request.Email);
        if (existingEmail != null)
            throw new ConflictException("Email já cadastrado.");

        var existingUserName = await userManager.FindByNameAsync(request.UserName);
        if (existingUserName != null)
            throw new ConflictException("UserName já cadastrado.");

        var email = new Email(request.Email);
        var user = new User(request.Nome, request.UserName, email.Endereco);

        var result = await userManager.CreateAsync(user, request.Senha);
        if (!result.Succeeded)
        {
            logger.LogWarning("Falha ao registrar usuário: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            throw new BadRequestException("Falha ao criar usuário.");
        }

        await userManager.AddToRoleAsync(user, "User");

        if (!string.IsNullOrEmpty(request.Cpf))
        {
            var cpf = new Cpf(request.Cpf);
            var data = await encryptionService.EncryptAsync(cpf.Numero);
            user.SetCpfData(data);
        }

        if (!string.IsNullOrEmpty(request.Rg))
        {
            var data = await encryptionService.EncryptAsync(request.Rg);
            user.SetRgData(data);
        }

        if (request.Endereco != null && !request.Endereco.IsEmpty)
        {
            var json = JsonSerializer.Serialize(request.Endereco);
            var data = await encryptionService.EncryptAsync(json);
            var address = new Address(user.Id);
            address.SetEncryptedData(data);
            await addressRepository.AddAsync(address);
        }

        var refreshToken = await CreateRefreshTokenAsync(user.Id);
        await unitOfWork.SaveChangesAsync();

        var (token, roles) = await GenerateTokenWithRolesAsync(user);

        logger.LogInformation("Usuário {UserId} registrado com sucesso", user.Id);
        return BuildAuthResponse(token, refreshToken, user, roles);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await userRepository.GetByEmailAsync(request.Email);
        if (user == null)
        {
            await userManager.CheckPasswordAsync(IdentityUserPlaceholder(), "fake");
            throw new UnauthorizedAccessException(InvalidEmailOrPassword);
        }

        if (await userManager.IsLockedOutAsync(user))
            throw new UnauthorizedAccessException("Conta temporariamente bloqueada por muitas tentativas inválidas. Tente novamente em 15 minutos.");

        var passwordValid = await userManager.CheckPasswordAsync(user, request.Senha);
        if (!passwordValid)
        {
            await userManager.AccessFailedAsync(user);
            logger.LogWarning("Tentativa de login inválida para usuário {UserId}", user.Id);
            throw new UnauthorizedAccessException(InvalidEmailOrPassword);
        }

        await userManager.ResetAccessFailedCountAsync(user);

        var refreshToken = await CreateRefreshTokenAsync(user.Id);
        await unitOfWork.SaveChangesAsync();

        var (token, roles) = await GenerateTokenWithRolesAsync(user);

        logger.LogInformation("Usuário {UserId} fez login", user.Id);
        return BuildAuthResponse(token, refreshToken, user, roles);
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        var tokenHash = tokenService.HashToken(refreshToken);
        var storedToken = await refreshTokenRepository.GetByTokenHashAsync(tokenHash);

        if (storedToken is null)
            throw new UnauthorizedAccessException(InvalidRefreshToken);

        if (storedToken.IsRevoked)
        {
            logger.LogWarning("Possível roubo de refresh token detectado para usuário {UserId}. Revogando todos os tokens.", storedToken.UserId);
            await RevokeRefreshTokensAsync(storedToken.UserId);
            throw new UnauthorizedAccessException(InvalidRefreshToken);
        }

        if (storedToken.IsExpired)
            throw new UnauthorizedAccessException(InvalidRefreshToken);

        storedToken.Revoke();
        await refreshTokenRepository.UpdateAsync(storedToken);

        var user = await userRepository.GetByIdAsync(storedToken.UserId);
        if (user is null)
            throw new UnauthorizedAccessException(UserNotFound);

        var newRefreshToken = await CreateRefreshTokenAsync(user.Id);
        await unitOfWork.SaveChangesAsync();

        var (token, roles) = await GenerateTokenWithRolesAsync(user);

        return BuildAuthResponse(token, newRefreshToken, user, roles);
    }

    public async Task RevokeRefreshTokensAsync(Guid userId)
    {
        var activeTokens = await refreshTokenRepository.GetActiveByUserIdAsync(userId);
        foreach (var token in activeTokens)
        {
            token.Revoke();
            await refreshTokenRepository.UpdateAsync(token);
        }
        await unitOfWork.SaveChangesAsync();
    }

    private async Task<string> CreateRefreshTokenAsync(Guid userId)
    {
        var (refreshToken, tokenHash) = tokenService.GenerateRefreshToken();
        await refreshTokenRepository.AddAsync(new RefreshToken(userId, tokenHash, DateTime.UtcNow + RefreshTokenExpiry));
        return refreshToken;
    }

    private async Task<(string Token, IList<string> Roles)> GenerateTokenWithRolesAsync(User user)
    {
        var roles = (await userManager.GetRolesAsync(user)).ToList();
        var token = tokenService.GenerateToken(user.Id, user.Email!, user.Nome, roles);
        return (token, roles);
    }

    private static AuthResponse BuildAuthResponse(string token, string refreshToken, User user, IList<string> roles) => new()
    {
        Token = token,
        RefreshToken = refreshToken,
        Email = user.Email!,
        Nome = user.Nome,
        UserName = user.UserName!,
        Roles = roles
    };

    private static User IdentityUserPlaceholder() => new("", "", "");
}
