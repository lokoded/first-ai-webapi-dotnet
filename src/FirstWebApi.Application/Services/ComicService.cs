using FirstWebApi.Application.DTOs.Request;
using FirstWebApi.Application.DTOs.Response;
using FirstWebApi.Application.Interfaces;
using FirstWebApi.Domain.Entities;
using FirstWebApi.Domain.Interfaces;

namespace FirstWebApi.Application.Services;

public class ComicService : IComicService
{
    private readonly IComicRepository _comicRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ComicService(IComicRepository comicRepository, IUnitOfWork unitOfWork)
    {
        _comicRepository = comicRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PaginatedResult<ComicResponse>> GetAllAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        var allComics = await _comicRepository.GetByUserIdAsync(userId);
        var totalCount = allComics.Count;
        var items = allComics
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(MapToResponse)
            .ToList();

        return new PaginatedResult<ComicResponse>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ComicResponse?> GetByIdAsync(Guid id, Guid userId)
    {
        var comic = await _comicRepository.GetByIdAsync(id);
        if (comic == null || comic.UserId != userId)
            return null;
        return MapToResponse(comic);
    }

    public async Task<ComicResponse> CreateAsync(ComicRequest request, Guid userId)
    {
        var comic = new Comic(request.Titulo, request.WebUrl, userId, request.ComicTypeId, request.Observacao);
        await _comicRepository.AddAsync(comic);
        await _unitOfWork.SaveChangesAsync();
        var saved = await _comicRepository.GetByIdAsync(comic.Id);
        return MapToResponse(saved!);
    }

    public async Task<ComicResponse?> UpdateAsync(Guid id, ComicRequest request, Guid userId)
    {
        var comic = await _comicRepository.GetByIdAsync(id);
        if (comic == null || comic.UserId != userId)
            return null;

        comic.Update(request.Titulo, request.WebUrl, request.ComicTypeId, request.Observacao);
        await _comicRepository.UpdateAsync(comic);
        await _unitOfWork.SaveChangesAsync();
        var saved = await _comicRepository.GetByIdAsync(comic.Id);
        return MapToResponse(saved!);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId)
    {
        var comic = await _comicRepository.GetByIdAsync(id);
        if (comic == null || comic.UserId != userId)
            return false;

        await _comicRepository.DeleteAsync(comic);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    private static ComicResponse MapToResponse(Comic comic)
    {
        return new ComicResponse
        {
            Id = comic.Id,
            Titulo = comic.Titulo,
            WebUrl = comic.WebUrl,
            Observacao = comic.Observacao,
            ComicTypeId = comic.ComicTypeId,
            ComicTypeNome = comic.ComicType?.Nome ?? string.Empty,
            CreatedAt = comic.CreatedAt,
            UpdatedAt = comic.UpdatedAt
        };
    }
}
