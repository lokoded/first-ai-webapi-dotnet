using FirstWebApi.Application.DTOs.Response;
using FirstWebApi.Application.Interfaces;
using FirstWebApi.Domain.Entities;
using FirstWebApi.Domain.Interfaces;

namespace FirstWebApi.Application.Services;

public class ComicTypeService : IComicTypeService
{
    private readonly IComicTypeRepository _comicTypeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ComicTypeService(IComicTypeRepository comicTypeRepository, IUnitOfWork unitOfWork)
    {
        _comicTypeRepository = comicTypeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<ComicTypeResponse>> GetAllAsync()
    {
        var types = await _comicTypeRepository.GetAllAsync();
        return types.Select(t => new ComicTypeResponse { Id = t.Id, Nome = t.Nome }).ToList();
    }

    public async Task<ComicTypeResponse> CreateAsync(string nome)
    {
        var comicType = new ComicType(nome);
        await _comicTypeRepository.AddAsync(comicType);
        await _unitOfWork.SaveChangesAsync();
        return new ComicTypeResponse { Id = comicType.Id, Nome = comicType.Nome };
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var comicType = await _comicTypeRepository.GetByIdAsync(id);
        if (comicType == null)
            return false;

        if (await _comicTypeRepository.HasComicsAsync(id))
            throw new InvalidOperationException("Tipo possui comics vinculadas. Remova-as primeiro.");

        await _comicTypeRepository.DeleteAsync(comicType);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}
