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
        var newMember = new MemberEntity(
            0,
            entity.Name,
            entity.Email,
            entity.Phone,
            entity.ProfileImage
        );
        return await _repository.AddAsync(newMember);
    }

    public async Task<MemberEntity?> UpdateAsync(int id, int loggedMember, MemberEntity model)
    {
        if (id != loggedMember)
            throw new UnauthorizedAccessException("Member is not authorized to update this resource.");

        var entity = await _repository.GetByIdAsync(id);
        if (entity is null)
            return null;

        // Update properties based on the model
        entity.Name = model.Name;
        entity.Email = model.Email;
        entity.Phone = model.Phone;
        entity.ProfileImage = model.ProfileImage;

        await _repository.UpdateAsync(entity);
        return entity;
    }

    public async Task<bool> DeleteAsync(int id, int loggedMember)
    {
        if (id != loggedMember)
            throw new UnauthorizedAccessException("Member is not authorized to delete this resource.");

        var entity = await _repository.GetByIdAsync(id);
        if (entity is null)
            return false;
        return await _repository.DeleteAsync(entity);
    }
}
