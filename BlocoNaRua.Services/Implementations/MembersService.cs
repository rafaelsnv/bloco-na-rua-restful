using BlocoNaRua.Data.Repositories.Interfaces;
using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Services.Interfaces;

namespace BlocoNaRua.Services.Implementations;

public class MembersService(IMembersRepository repository) : IMembersService
{
    private readonly IMembersRepository _repository = repository;

    public async Task<IList<MemberEntity>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<MemberEntity?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<MemberEntity> CreateAsync(MemberEntity entity)
    {
        // Assuming MemberEntity has properties like Name, Role, etc.
        // The constructor for MemberEntity is not provided, so we'll assume a default constructor or properties can be set directly.
        // If MemberEntity has specific constructor parameters, they would need to be passed here.
        // For now, we'll create a new entity with the provided data.
        var newMember = new MemberEntity(
            0, // Assuming ID is auto-generated
            entity.Name,
            entity.Email,
            entity.Phone,
            entity.ProfileImage
        );
        return await _repository.AddAsync(newMember);
    }

    public async Task<MemberEntity?> UpdateAsync(int id, MemberEntity model)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null)
            return null;

        // Update properties based on the model
        entity.Name = model.Name;
        entity.Email = model.Email;
        entity.Phone = model.Phone;
        entity.ProfileImage = model.ProfileImage;
        // Update other properties as needed

        await _repository.UpdateAsync(entity);
        return entity;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null)
            return false;
        return await _repository.DeleteAsync(entity);
    }
}
