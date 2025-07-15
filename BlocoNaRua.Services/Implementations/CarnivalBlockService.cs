using BlocoNaRua.Data.Repositories.Interfaces;
using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Restful.Models.CarnivalBlock;
using BlocoNaRua.Restful.Services.Interfaces;

namespace BlocoNaRua.Services.Implementations;
public class CarnivalBlockService(ICarnivalBlocksRepository repository) : ICarnivalBlockService
{
    private readonly ICarnivalBlocksRepository _repository = repository;

    public async Task<IEnumerable<CarnivalBlockDTO>> GetAll()
    {
        return await _repository.GetAllAsync();
    }

    public CarnivalBlockDTO GetById(int id)
    {
        return _repository.GetByIdAsync(id);
    }

    public void Create(CarnivalBlockCreate model)
    {
        var entity = new CarnivalBlockEntity
        {
            Name = model.Name,
            Description = model.Description,
            // outros campos...
        };
        _repository.Add(entity);
    }

    public void Update(int id, CarnivalBlockUpdate model)
    {
        var entity = _repository.GetById(id);
        if (entity == null) throw new Exception("Bloco não encontrado");

        entity.Name = model.Name;
        entity.Description = model.Description;
        // outros campos...

        _repository.Update(entity);
    }

    public void Delete(int id)
    {
        _repository.Delete(id);
    }
}
