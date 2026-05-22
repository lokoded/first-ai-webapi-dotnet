using System.Security.Claims;
using FirstWebApi.Application.DTOs.Request;
using FirstWebApi.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FirstWebApi.WebApi.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
[EnableRateLimiting("Default")]
public class UsersController(
    IProfileService profileService,
    IValidator<FullProfileRequest> fullProfileValidator) : ControllerBase
{

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

        var profile = await profileService.GetProfileAsync(userId);
        return Ok(profile);
    }

    [HttpPost("me/full")]
    [EnableRateLimiting("Strict")]
    public async Task<IActionResult> GetFullProfile([FromBody] FullProfileRequest request)
    {
        var validation = await fullProfileValidator.ValidateAsync(request);
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
            var profile = await profileService.GetFullProfileAsync(userId, request.Senha);
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
