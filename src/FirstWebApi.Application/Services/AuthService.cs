using System.Text.Json;
using FirstWebApi.Application.DTOs;
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
            var erros = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogWarning("Falha ao registrar usuário {Email}: {Erros}", request.Email, erros);
            throw new InvalidOperationException($"Falha ao criar usuário: {erros}");
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
            if (request.Rg.Length < 4 || request.Rg.Length > 20)
                throw new ArgumentException("RG inválido.");
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
        var refreshTokenEntity = new RefreshToken(user.Id, tokenHash, DateTime.UtcNow.AddDays(7));
        await _refreshTokenRepository.AddAsync(refreshTokenEntity);

        await _unitOfWork.SaveChangesAsync();

        var roles = (await _userManager.GetRolesAsync(user)).ToList();
        var token = _tokenService.GenerateToken(user.Id, user.Email!, user.Nome, roles);

        _logger.LogInformation("Usuário {UserId} registrado com sucesso", user.Id);

        return new AuthResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            Email = user.Email!,
            Nome = user.Nome,
            UserName = user.UserName!,
            Roles = roles
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
            throw new UnauthorizedAccessException("Email ou senha inválidos.");

        var senhaValida = await _userManager.CheckPasswordAsync(user, request.Senha);
        if (!senhaValida)
        {
            _logger.LogWarning("Tentativa de login inválida para {Email}", request.Email);
            throw new UnauthorizedAccessException("Email ou senha inválidos.");
        }

        var (refreshToken, tokenHash) = _tokenService.GenerateRefreshToken();
        var refreshTokenEntity = new RefreshToken(user.Id, tokenHash, DateTime.UtcNow.AddDays(7));
        await _refreshTokenRepository.AddAsync(refreshTokenEntity);
        await _unitOfWork.SaveChangesAsync();

        var roles = (await _userManager.GetRolesAsync(user)).ToList();
        var token = _tokenService.GenerateToken(user.Id, user.Email!, user.Nome, roles);

        _logger.LogInformation("Usuário {UserId} fez login", user.Id);

        return new AuthResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            Email = user.Email!,
            Nome = user.Nome,
            UserName = user.UserName!,
            Roles = roles
        };
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        var tokenHash = _tokenService.HashToken(refreshToken);
        var storedToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash);

        if (storedToken is null || !storedToken.IsActive)
            throw new UnauthorizedAccessException("Refresh token inválido ou expirado.");

        storedToken.Revoke();
        _refreshTokenRepository.Update(storedToken);

        var user = await _userRepository.GetByIdAsync(storedToken.UserId);
        if (user is null)
            throw new UnauthorizedAccessException("Usuário não encontrado.");

        var (newRefreshToken, newTokenHash) = _tokenService.GenerateRefreshToken();
        var newRefreshTokenEntity = new RefreshToken(user.Id, newTokenHash, DateTime.UtcNow.AddDays(7));
        await _refreshTokenRepository.AddAsync(newRefreshTokenEntity);
        await _unitOfWork.SaveChangesAsync();

        var roles = (await _userManager.GetRolesAsync(user)).ToList();
        var token = _tokenService.GenerateToken(user.Id, user.Email!, user.Nome, roles);

        return new AuthResponse
        {
            Token = token,
            RefreshToken = newRefreshToken,
            Email = user.Email!,
            Nome = user.Nome,
            UserName = user.UserName!,
            Roles = roles
        };
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

    public async Task<UserResponse> GetProfileAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("Usuário não encontrado.");

        var response = new UserResponse
        {
            Id = user.Id,
            Nome = user.Nome,
            UserName = user.UserName!,
            Email = user.Email!,
            Role = user.Role.ToString(),
            CreatedAt = user.CreatedAt
        };

        if (user.CpfCiphertext != null && user.CpfIv != null && user.CpfTag != null && user.CpfEncryptedDataKey != null)
        {
            try
            {
                response.Cpf = await _encryptionService.DecryptAsync(
                    user.CpfCiphertext, user.CpfIv, user.CpfTag, user.CpfEncryptedDataKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao descriptografar CPF do usuário {UserId}", userId);
            }
        }

        if (user.RgCiphertext != null && user.RgIv != null && user.RgTag != null && user.RgEncryptedDataKey != null)
        {
            try
            {
                response.Rg = await _encryptionService.DecryptAsync(
                    user.RgCiphertext, user.RgIv, user.RgTag, user.RgEncryptedDataKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao descriptografar RG do usuário {UserId}", userId);
            }
        }

        var address = await _addressRepository.GetByUserIdAsync(userId);
        if (address?.Ciphertext != null && address?.Iv != null && address?.Tag != null && address?.EncryptedDataKey != null)
        {
            try
            {
                var enderecoJson = await _encryptionService.DecryptAsync(
                    address.Ciphertext, address.Iv, address.Tag, address.EncryptedDataKey);
                response.Endereco = JsonSerializer.Deserialize<EnderecoInfo>(enderecoJson);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao descriptografar endereço do usuário {UserId}", userId);
            }
        }

        return response;
    }
}

