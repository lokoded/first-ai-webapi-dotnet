using System.Security.Claims;
using FirstWebApi.Application.DTOs.Request;
using FirstWebApi.Application.DTOs.Response;
using FirstWebApi.Application.Interfaces;
using FirstWebApi.Application.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FirstWebApi.WebApi.Controllers;

[ApiController]
[Route("api/comics")]
[Authorize]
[EnableRateLimiting("Default")]
public class ComicsController : ControllerBase
{
    private readonly IComicService _comicService;

    public ComicsController(IComicService comicService)
    {
        _comicService = comicService;
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<ComicResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
            return Problem(detail: "Token inválido.", statusCode: 401, title: "Não autorizado");

        var result = await _comicService.GetAllAsync(userId, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ComicResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
            return Problem(detail: "Token inválido.", statusCode: 401, title: "Não autorizado");

        var result = await _comicService.GetByIdAsync(id, userId);
        if (result is null)
            return Problem(detail: "Comic não encontrada.", statusCode: 404, title: "Não encontrado");

        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ComicResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] ComicRequest request)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
            return Problem(detail: "Token inválido.", statusCode: 401, title: "Não autorizado");

        var validator = new ComicRequestValidator();
        var validation = await validator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            var errors = validation.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            return ValidationProblem(new ValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest
            });
        }

        var result = await _comicService.CreateAsync(request, userId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ComicResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(Guid id, [FromBody] ComicRequest request)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
            return Problem(detail: "Token inválido.", statusCode: 401, title: "Não autorizado");

        var validator = new ComicRequestValidator();
        var validation = await validator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            var errors = validation.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            return ValidationProblem(new ValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest
            });
        }

        var result = await _comicService.UpdateAsync(id, request, userId);
        if (result is null)
            return Problem(detail: "Comic não encontrada.", statusCode: 404, title: "Não encontrado");

        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
            return Problem(detail: "Token inválido.", statusCode: 401, title: "Não autorizado");

        var success = await _comicService.DeleteAsync(id, userId);
        if (!success)
            return Problem(detail: "Comic não encontrada.", statusCode: 404, title: "Não encontrado");

        return NoContent();
    }
}
