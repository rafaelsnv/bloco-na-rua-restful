using BlocoNaRua.Data.Repositories.Interfaces;
using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Domain.Enums;
using BlocoNaRua.Services.Interfaces;

namespace BlocoNaRua.Services.Implementations;

public class CarnivalBlockService
(
    ICarnivalBlocksRepository repository,
    ICarnivalBlockMembersRepository carnivalBlockMembersRepository
) : ICarnivalBlockService
{
    private readonly ICarnivalBlocksRepository _repository = repository;
    private readonly ICarnivalBlockMembersRepository _carnivalBlockMembersRepository = carnivalBlockMembersRepository;

    public async Task<IList<CarnivalBlockEntity>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<CarnivalBlockEntity?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<CarnivalBlockEntity> CreateAsync(CarnivalBlockEntity model)
    {
        var entity = new CarnivalBlockEntity
        (
            0,
            model.OwnerId,
            model.Name,
            string.Empty,
            string.Empty,
            model.CarnivalBlockImage
        );
        return await _repository.AddAsync(entity);
    }

    public async Task<CarnivalBlockEntity?> UpdateAsync(int id, int memberId, CarnivalBlockEntity model)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null)
            return null;

        var memberRole = await _carnivalBlockMembersRepository.GetMemberRole(id, memberId);

        if (memberRole != RolesEnum.Owner && memberRole != RolesEnum.Manager)
        {
            throw new UnauthorizedAccessException("Member is not authorized to update this carnival block.");
        }

        entity.Name = model.Name;
        entity.CarnivalBlockImage = model.CarnivalBlockImage;
        await _repository.UpdateAsync(entity);
        return entity;
    }

    public async Task<bool> DeleteAsync(int id, int memberId)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null)
            return false;

        var memberRole = await _carnivalBlockMembersRepository.GetMemberRole(id, memberId);

        if (memberRole != RolesEnum.Owner)
        {
            throw new UnauthorizedAccessException("Member is not authorized to delete this carnival block.");
        }

        return await _repository.DeleteAsync(entity);
    }
}
