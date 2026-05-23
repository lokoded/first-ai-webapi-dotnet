using FirstWebApi.Application.DTOs.Request;
using FirstWebApi.Application.DTOs.Response;
using FirstWebApi.Application.Interfaces;
using FirstWebApi.WebApi.Extensions;
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
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetProfile()
    {
        var userId = User.GetUserId();
        if (userId == Guid.Empty)
            return Problem(
                detail: "Token inválido.",
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Não autorizado");

        var profile = await profileService.GetProfileAsync(userId, HttpContext.RequestAborted);
        return Ok(profile);
    }

    [HttpPost("me/full")]
    [EnableRateLimiting("Strict")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetFullProfile([FromBody] FullProfileRequest request)
    {
        var userId = User.GetUserId();
        if (userId == Guid.Empty)
            return Problem(
                detail: "Token inválido.",
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Não autorizado");

        var profile = await profileService.GetFullProfileAsync(userId, request.Senha, HttpContext.RequestAborted);
        return Ok(profile);
    }
}
