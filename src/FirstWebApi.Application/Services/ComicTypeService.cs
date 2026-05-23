using FirstWebApi.Application.DTOs.Response;
using FirstWebApi.Application.Exceptions;
using FirstWebApi.Application.Interfaces;
using FirstWebApi.Domain.Entities;
using FirstWebApi.Domain.Interfaces;

namespace FirstWebApi.Application.Services;

public class ComicTypeService(IComicTypeRepository comicTypeRepository, IUnitOfWork unitOfWork) : IComicTypeService
{

    public async Task<List<ComicTypeResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var types = await comicTypeRepository.GetAllAsync(cancellationToken);
        return types.Select(t => new ComicTypeResponse { Id = t.Id, Nome = t.Nome }).ToList();
    }

    public async Task<ComicTypeResponse> CreateAsync(string name, CancellationToken cancellationToken = default)
    {
        var comicType = new ComicType(name);
        await comicTypeRepository.AddAsync(comicType, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new ComicTypeResponse { Id = comicType.Id, Nome = comicType.Nome };
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var comicType = await comicTypeRepository.GetByIdAsync(id, cancellationToken);
        if (comicType == null)
            return false;

        if (await comicTypeRepository.HasComicsAsync(id, cancellationToken))
            throw new ConflictException("Tipo possui comics vinculadas. Remova-as primeiro.");

        await comicTypeRepository.DeleteAsync(comicType, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
