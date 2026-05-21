using FirstWebApi.Application.DTOs.Response;
using FirstWebApi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FirstWebApi.WebApi.Controllers;

[ApiController]
[Route("api/comic-types")]
[Authorize]
[EnableRateLimiting("Default")]
public class ComicTypesController : ControllerBase
{
    private readonly IComicTypeService _comicTypeService;

    public ComicTypesController(IComicTypeService comicTypeService)
    {
        _comicTypeService = comicTypeService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<ComicTypeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _comicTypeService.GetAllAsync();
        return Ok(result);
    }
}
