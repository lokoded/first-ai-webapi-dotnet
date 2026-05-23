using FirstWebApi.Application.DTOs.Request;
using FirstWebApi.Application.Interfaces;
using FirstWebApi.WebApi.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FirstWebApi.WebApi.Controllers;

[ApiController]
[Route("api/auth")]
[EnableRateLimiting("Auth")]
public class AuthController(
    IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await authService.RegisterAsync(request);
        SetRefreshTokenCookie(result.RefreshToken);
        return CreatedAtAction(nameof(Register), new { email = result.Email }, result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await authService.LoginAsync(request);
        SetRefreshTokenCookie(result.RefreshToken);
        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var refreshToken = Request.Cookies["RefreshToken"];
        if (string.IsNullOrEmpty(refreshToken))
            return Problem(detail: "Refresh token não encontrado.", statusCode: StatusCodes.Status401Unauthorized, title: "Não autorizado");

        var result = await authService.RefreshTokenAsync(refreshToken);
        SetRefreshTokenCookie(result.RefreshToken);
        return Ok(result);
    }

    [HttpPost("revoke")]
    [Authorize]
    public async Task<IActionResult> Revoke()
    {
        var userId = User.GetUserId();
        if (userId == Guid.Empty)
            return Problem(detail: "Token inválido.", statusCode: StatusCodes.Status401Unauthorized, title: "Não autorizado");

        await authService.RevokeRefreshTokensAsync(userId);
        return NoContent();
    }

    private void SetRefreshTokenCookie(string refreshToken)
    {
        var expires = DateTimeOffset.UtcNow.AddDays(7).ToString("R");
        var secure = Request.IsHttps ? "; Secure" : "";
        Response.Headers.Append("Set-Cookie", $"RefreshToken={refreshToken}; HttpOnly; SameSite=Strict; Expires={expires}{secure}");
    }
}
