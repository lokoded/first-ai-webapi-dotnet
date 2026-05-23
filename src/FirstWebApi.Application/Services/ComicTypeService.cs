using FirstWebApi.Application.DTOs.Response;
using FirstWebApi.Application.Exceptions;
using FirstWebApi.Application.Interfaces;
using FirstWebApi.Domain.Entities;
using FirstWebApi.Domain.Interfaces;

namespace FirstWebApi.Application.Services;

public class ComicTypeService(IComicTypeRepository comicTypeRepository, IUnitOfWork unitOfWork) : IComicTypeService
{

    public async Task<List<ComicTypeResponse>> GetAllAsync()
    {
        var types = await comicTypeRepository.GetAllAsync();
        return types.Select(t => new ComicTypeResponse { Id = t.Id, Nome = t.Nome }).ToList();
    }

    public async Task<ComicTypeResponse> CreateAsync(string nome)
    {
        var comicType = new ComicType(nome);
        await comicTypeRepository.AddAsync(comicType);
        await unitOfWork.SaveChangesAsync();
        return new ComicTypeResponse { Id = comicType.Id, Nome = comicType.Nome };
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var comicType = await comicTypeRepository.GetByIdAsync(id);
        if (comicType == null)
            return false;

        if (await comicTypeRepository.HasComicsAsync(id))
            throw new ConflictException("Tipo possui comics vinculadas. Remova-as primeiro.");

        await comicTypeRepository.DeleteAsync(comicType);
        await unitOfWork.SaveChangesAsync();
        return true;
    }
}
