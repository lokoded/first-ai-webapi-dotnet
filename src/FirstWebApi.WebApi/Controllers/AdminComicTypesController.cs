using FirstWebApi.Application.DTOs.Request;
using FirstWebApi.Application.DTOs.Response;
using FirstWebApi.Application.Interfaces;
using FirstWebApi.Application.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FirstWebApi.WebApi.Controllers;

[ApiController]
[Route("api/admin/comic-types")]
[Authorize(Roles = "Admin")]
[EnableRateLimiting("Default")]
public class AdminComicTypesController : ControllerBase
{
    private readonly IComicTypeService _comicTypeService;

    public AdminComicTypesController(IComicTypeService comicTypeService)
    {
        _comicTypeService = comicTypeService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ComicTypeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] ComicTypeRequest request)
    {
        var validator = new ComicTypeRequestValidator();
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

        var result = await _comicTypeService.CreateAsync(request.Nome);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _comicTypeService.DeleteAsync(id);
        if (!success)
            return Problem(detail: "Tipo de comic não encontrado.", statusCode: 404, title: "Não encontrado");

        return NoContent();
    }

}
