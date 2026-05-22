using System.Text.Json;
using FirstWebApi.Application.DTOs.Request;
using FirstWebApi.Application.DTOs.Response;
using FirstWebApi.Application.Interfaces;
using FirstWebApi.Domain.Entities;
using FirstWebApi.Domain.Interfaces;
using FirstWebApi.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace FirstWebApi.Application.Services;

public class AuthService : IAuthService
{
    private static readonly TimeSpan RefreshTokenExpiry = TimeSpan.FromDays(7);
    private const string InvalidEmailOrPassword = "Email ou senha inválidos.";
    private const string InvalidRefreshToken = "Refresh token inválido ou expirado.";
    private const string UserNotFound = "Usuário não encontrado.";

    private readonly UserManager<User> _userManager;
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IEncryptionService _encryptionService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AuthService> _logger;
    private readonly IAddressRepository _addressRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public AuthService(
        UserManager<User> userManager,
        IUserRepository userRepository,
        ITokenService tokenService,
        IEncryptionService encryptionService,
        IUnitOfWork unitOfWork,
        ILogger<AuthService> logger,
        IAddressRepository addressRepository,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _userManager = userManager;
        _userRepository = userRepository;
        _tokenService = tokenService;
        _encryptionService = encryptionService;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _addressRepository = addressRepository;
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var existingEmail = await _userRepository.GetByEmailAsync(request.Email);
        if (existingEmail != null)
            throw new InvalidOperationException("Email já cadastrado.");

        var existingUserName = await _userManager.FindByNameAsync(request.UserName);
        if (existingUserName != null)
            throw new InvalidOperationException("UserName já cadastrado.");

        var user = new User(request.Nome, request.UserName, request.Email);

        var result = await _userManager.CreateAsync(user, request.Senha);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogWarning("Falha ao registrar usuário: {Errors}", errors);
            throw new InvalidOperationException($"Falha ao criar usuário: {errors}");
        }

        await _userManager.AddToRoleAsync(user, "User");

        if (!string.IsNullOrEmpty(request.Cpf))
        {
            var cpf = new Cpf(request.Cpf);
            var (ciphertext, iv, tag, dk) = await _encryptionService.EncryptAsync(cpf.Numero);
            user.SetCpfData(ciphertext, iv, tag, dk);
        }

        if (!string.IsNullOrEmpty(request.Rg))
        {
            var (ciphertext, iv, tag, dk) = await _encryptionService.EncryptAsync(request.Rg);
            user.SetRgData(ciphertext, iv, tag, dk);
        }

        if (request.Endereco != null && !request.Endereco.IsEmpty)
        {
            var json = JsonSerializer.Serialize(request.Endereco);
            var (ciphertext, iv, tag, dk) = await _encryptionService.EncryptAsync(json);
            var address = new Address(user.Id);
            address.SetEncryptedData(ciphertext, iv, tag, dk);
            await _addressRepository.AddAsync(address);
        }

        var (refreshToken, tokenHash) = _tokenService.GenerateRefreshToken();
        var refreshTokenEntity = new RefreshToken(user.Id, tokenHash, DateTime.UtcNow + RefreshTokenExpiry);
        await _refreshTokenRepository.AddAsync(refreshTokenEntity);

        await _unitOfWork.SaveChangesAsync();

        var roles = (await _userManager.GetRolesAsync(user)).ToList();
        var token = _tokenService.GenerateToken(user.Id, user.Email!, user.Nome, roles);

        _logger.LogInformation("Usuário {UserId} registrado com sucesso", user.Id);

        return BuildAuthResponse(token, refreshToken, user, roles);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
        {
            await _userManager.CheckPasswordAsync(IdentityUserPlaceholder(), "fake");
            throw new UnauthorizedAccessException(InvalidEmailOrPassword);
        }

        if (await _userManager.IsLockedOutAsync(user))
            throw new UnauthorizedAccessException("Conta temporariamente bloqueada por muitas tentativas inválidas. Tente novamente em 15 minutos.");

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Senha);
        if (!passwordValid)
        {
            await _userManager.AccessFailedAsync(user);
            _logger.LogWarning("Tentativa de login inválida para usuário {UserId}", user.Id);
            throw new UnauthorizedAccessException(InvalidEmailOrPassword);
        }

        await _userManager.ResetAccessFailedCountAsync(user);

        var (refreshToken, tokenHash) = _tokenService.GenerateRefreshToken();
        var refreshTokenEntity = new RefreshToken(user.Id, tokenHash, DateTime.UtcNow + RefreshTokenExpiry);
        await _refreshTokenRepository.AddAsync(refreshTokenEntity);
        await _unitOfWork.SaveChangesAsync();

        var roles = (await _userManager.GetRolesAsync(user)).ToList();
        var token = _tokenService.GenerateToken(user.Id, user.Email!, user.Nome, roles);

        _logger.LogInformation("Usuário {UserId} fez login", user.Id);

        return BuildAuthResponse(token, refreshToken, user, roles);
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        var tokenHash = _tokenService.HashToken(refreshToken);
        var storedToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash);

        if (storedToken is null)
            throw new UnauthorizedAccessException(InvalidRefreshToken);

        if (storedToken.IsRevoked)
        {
            _logger.LogWarning("Possível roubo de refresh token detectado para usuário {UserId}. Revogando todos os tokens.", storedToken.UserId);
            await RevokeRefreshTokensAsync(storedToken.UserId);
            throw new UnauthorizedAccessException(InvalidRefreshToken);
        }

        if (storedToken.IsExpired)
            throw new UnauthorizedAccessException(InvalidRefreshToken);

        storedToken.Revoke();
        _refreshTokenRepository.Update(storedToken);

        var user = await _userRepository.GetByIdAsync(storedToken.UserId);
        if (user is null)
            throw new UnauthorizedAccessException(UserNotFound);

        var (newRefreshToken, newTokenHash) = _tokenService.GenerateRefreshToken();
        var newRefreshTokenEntity = new RefreshToken(user.Id, newTokenHash, DateTime.UtcNow + RefreshTokenExpiry);
        await _refreshTokenRepository.AddAsync(newRefreshTokenEntity);
        await _unitOfWork.SaveChangesAsync();

        var roles = (await _userManager.GetRolesAsync(user)).ToList();
        var token = _tokenService.GenerateToken(user.Id, user.Email!, user.Nome, roles);

        return BuildAuthResponse(token, newRefreshToken, user, roles);
    }

    public async Task RevokeRefreshTokensAsync(Guid userId)
    {
        var activeTokens = await _refreshTokenRepository.GetActiveByUserIdAsync(userId);
        foreach (var token in activeTokens)
        {
            token.Revoke();
            _refreshTokenRepository.Update(token);
        }
        await _unitOfWork.SaveChangesAsync();
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
