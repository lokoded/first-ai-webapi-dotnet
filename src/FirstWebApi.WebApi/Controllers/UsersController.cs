using System.Security.Claims;
using FirstWebApi.Application.DTOs.Request;
using FirstWebApi.Application.Interfaces;
using FirstWebApi.Application.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FirstWebApi.WebApi.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
[EnableRateLimiting("Default")]
public class UsersController : ControllerBase
{
    private readonly IAuthService _authService;

    public UsersController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetProfile()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Problem(
                detail: "Token inválido.",
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Não autorizado");

        var profile = await _authService.GetProfileAsync(userId);
        return Ok(profile);
    }

    [HttpPost("me/full")]
    public async Task<IActionResult> GetFullProfile([FromBody] FullProfileRequest request)
    {
        var validator = new FullProfileRequestValidator();
        var validation = await validator.ValidateAsync(request);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Problem(
                detail: "Token inválido.",
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Não autorizado");

        try
        {
            var profile = await _authService.GetFullProfileAsync(userId, request.Senha);
            return Ok(profile);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status403Forbidden,
                title: "Reautenticação necessária");
        }
    }
}
