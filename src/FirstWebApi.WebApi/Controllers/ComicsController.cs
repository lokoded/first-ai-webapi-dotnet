using FirstWebApi.Application.DTOs.Request;
using FirstWebApi.Application.DTOs.Response;
using FirstWebApi.Application.Interfaces;
using FirstWebApi.WebApi.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FirstWebApi.WebApi.Controllers;

[ApiController]
[Route("api/comics")]
[Authorize]
[EnableRateLimiting("Default")]
public class ComicsController(
    IComicService comicService,
    IValidator<ComicRequest> validator) : ControllerBase
{

    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<ComicResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = User.GetUserId();
        if (userId == Guid.Empty)
            return Problem(detail: "Token inválido.", statusCode: StatusCodes.Status401Unauthorized, title: "Não autorizado");

        var result = await comicService.GetAllAsync(userId, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ComicResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId = User.GetUserId();
        if (userId == Guid.Empty)
            return Problem(detail: "Token inválido.", statusCode: StatusCodes.Status401Unauthorized, title: "Não autorizado");

        var result = await comicService.GetByIdAsync(id, userId);
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
        var userId = User.GetUserId();
        if (userId == Guid.Empty)
            return Problem(detail: "Token inválido.", statusCode: StatusCodes.Status401Unauthorized, title: "Não autorizado");

        var validation = await validator.ValidateAsync(request);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));

        var result = await comicService.CreateAsync(request, userId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(Guid id, [FromBody] ComicRequest request)
    {
        var userId = User.GetUserId();
        if (userId == Guid.Empty)
            return Problem(detail: "Token inválido.", statusCode: StatusCodes.Status401Unauthorized, title: "Não autorizado");

        var validation = await validator.ValidateAsync(request);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));

        var result = await comicService.UpdateAsync(id, request, userId);
        if (result is null)
            return Problem(detail: "Comic não encontrada.", statusCode: 404, title: "Não encontrado");

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = User.GetUserId();
        if (userId == Guid.Empty)
            return Problem(detail: "Token inválido.", statusCode: StatusCodes.Status401Unauthorized, title: "Não autorizado");

        var success = await comicService.DeleteAsync(id, userId);
        if (!success)
            return Problem(detail: "Comic não encontrada.", statusCode: 404, title: "Não encontrado");

        return NoContent();
    }
}
