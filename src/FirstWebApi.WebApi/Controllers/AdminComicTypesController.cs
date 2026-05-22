using FirstWebApi.Application.DTOs.Request;
using FirstWebApi.Application.DTOs.Response;
using FirstWebApi.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.RateLimiting;

namespace FirstWebApi.WebApi.Controllers;

[ApiController]
[Route("api/admin/comic-types")]
[Authorize(Roles = "Admin")]
[EnableRateLimiting("Default")]
public class AdminComicTypesController(
    IComicTypeService comicTypeService,
    IValidator<ComicTypeRequest> validator) : ControllerBase
{

    [HttpPost]
    [ProducesResponseType(typeof(ComicTypeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] ComicTypeRequest request)
    {
        var validation = await validator.ValidateAsync(request);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));

        var result = await comicTypeService.CreateAsync(request.Nome);
        return Created($"/api/admin/comic-types/{result.Id}", result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await comicTypeService.DeleteAsync(id);
        if (!success)
            return Problem(detail: "Tipo de comic não encontrado.", statusCode: 404, title: "Não encontrado");

        return NoContent();
    }
}
