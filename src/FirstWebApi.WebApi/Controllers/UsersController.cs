using FirstWebApi.Application.DTOs.Request;
using FirstWebApi.Application.Interfaces;
using FirstWebApi.WebApi.Extensions;
using FirstWebApi.WebApi.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FirstWebApi.WebApi.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
[EnableRateLimiting("Default")]
public class UsersController(
    IProfileService profileService) : ControllerBase
{

    [HttpGet("me")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = User.GetUserId();
        if (userId == Guid.Empty)
            return Problem(
                detail: "Token inválido.",
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Não autorizado");

        var profile = await profileService.GetProfileAsync(userId);
        MaskingHelper.ApplyMask(profile);
        return Ok(profile);
    }

    [HttpPost("me/full")]
    [EnableRateLimiting("Strict")]
    public async Task<IActionResult> GetFullProfile([FromBody] FullProfileRequest request)
    {
        var userId = User.GetUserId();
        if (userId == Guid.Empty)
            return Problem(
                detail: "Token inválido.",
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Não autorizado");

        var profile = await profileService.GetFullProfileAsync(userId, request.Senha);
        return Ok(profile);
    }
}
