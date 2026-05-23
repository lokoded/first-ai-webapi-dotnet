using FirstWebApi.Application.DTOs.Request;
using FirstWebApi.Application.DTOs.Response;
using FirstWebApi.Application.Exceptions;
using FirstWebApi.Application.Interfaces;
using FirstWebApi.Domain.Entities;
using FirstWebApi.Domain.Interfaces;

namespace FirstWebApi.Application.Services;

public class ComicService(
    IComicRepository comicRepository,
    IUnitOfWork unitOfWork,
    IComicTypeRepository comicTypeRepository) : IComicService
{
    public async Task<PaginatedResult<ComicResponse>> GetAllAsync(Guid userId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        var (items, totalCount) = await comicRepository.GetPaginatedByUserIdAsync(userId, page, pageSize, cancellationToken);

        return new PaginatedResult<ComicResponse>
        {
            Data = items.Select(MapToResponse).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ComicResponse?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var comic = await comicRepository.GetByIdAsync(id, cancellationToken);
        if (comic == null || comic.UserId != userId)
            return null;
        return MapToResponse(comic);
    }

    public async Task<ComicResponse> CreateAsync(ComicRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Titulo))
            throw new BadRequestException("Título é obrigatório.");

        if (!Uri.TryCreate(request.WebUrl, UriKind.Absolute, out _))
            throw new BadRequestException("WebUrl deve ser uma URL válida.");

        var comicType = await comicTypeRepository.GetByIdAsync(request.ComicTypeId, cancellationToken);
        if (comicType is null)
            throw new KeyNotFoundException("Tipo de quadrinho não encontrado.");

        var comic = new Comic(request.Titulo, request.WebUrl, userId, request.ComicTypeId, request.Observacao);
        await comicRepository.AddAsync(comic, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return MapToResponse(comic);
    }

    public async Task<ComicResponse?> UpdateAsync(Guid id, ComicRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var comic = await comicRepository.GetByIdAsync(id, cancellationToken);
        if (comic == null || comic.UserId != userId)
            return null;

        if (request.ComicTypeId != comic.ComicTypeId)
        {
            var comicType = await comicTypeRepository.GetByIdAsync(request.ComicTypeId, cancellationToken);
            if (comicType is null)
                throw new KeyNotFoundException("Tipo de quadrinho não encontrado.");
        }

        comic.Update(request.Titulo, request.WebUrl, request.ComicTypeId, request.Observacao);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return MapToResponse(comic);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var comic = await comicRepository.GetByIdAsync(id, cancellationToken);
        if (comic == null || comic.UserId != userId)
            return false;

        await comicRepository.DeleteAsync(comic, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static ComicResponse MapToResponse(Comic comic) => new()
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
