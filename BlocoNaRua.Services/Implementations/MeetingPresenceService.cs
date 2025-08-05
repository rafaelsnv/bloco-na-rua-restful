using BlocoNaRua.Data.Repositories.Interfaces;
using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Domain.Enums;
using BlocoNaRua.Services.Interfaces;

namespace BlocoNaRua.Services.Implementations;

public class MeetingPresenceService
(
    IMeetingPresencesRepository repository,
    ICarnivalBlocksRepository carnivalBlocksRepository,
    ICarnivalBlockMembersRepository carnivalBlockMembersRepository
) : IMeetingPresenceService
{
    private readonly IMeetingPresencesRepository _repository = repository;
    private readonly ICarnivalBlocksRepository _carnivalBlocksRepository = carnivalBlocksRepository;
    private readonly ICarnivalBlockMembersRepository _carnivalBlockMembersRepository = carnivalBlockMembersRepository;

    public async Task<IList<MeetingPresenceEntity>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<MeetingPresenceEntity?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<MeetingPresenceEntity> CreateAsync(MeetingPresenceEntity model, int loggedMember)
    {

        if (model.MemberId == loggedMember)
        {
            return await _repository.AddAsync(model);
        }

        var memberRole = await GetMemberRole(model.CarnivalBlockId, loggedMember);
        if (memberRole == RolesEnum.Owner || memberRole == RolesEnum.Manager)
        {
            return await _repository.AddAsync(model);
        }

        throw new UnauthorizedAccessException("You are not authorized to create a meeting presence for another member.");
    }

    public async Task<MeetingPresenceEntity?> UpdateAsync(int id, MeetingPresenceEntity model, int loggedMember)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Meeting presence does not exist.");

        if (entity.MemberId == loggedMember)
        {
            entity.IsPresent = model.IsPresent;
            await _repository.UpdateAsync(entity);
            return entity;
        }

        var memberRole = await GetMemberRole(entity.CarnivalBlockId, loggedMember);
        if (memberRole == RolesEnum.Owner || memberRole == RolesEnum.Manager)
        {
            entity.IsPresent = model.IsPresent;
            await _repository.UpdateAsync(entity);
            return entity;
        }

        throw new UnauthorizedAccessException("You are not authorized to update this meeting presence.");
    }

    public async Task<bool> DeleteAsync(int id, int loggedMember)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Meeting presence does not exist.");

        if (entity.MemberId == loggedMember)
        {
            return await _repository.DeleteAsync(entity);
        }

        var memberRole = await GetMemberRole(entity.CarnivalBlockId, loggedMember);
        if (memberRole == RolesEnum.Owner || memberRole == RolesEnum.Manager)
        {
            return await _repository.DeleteAsync(entity);
        }

        throw new UnauthorizedAccessException("You are not authorized to delete this meeting presence.");
    }

    private async Task<RolesEnum?> GetMemberRole(int carnivalBlockId, int memberId)
    {
        var carnivalBlock = await _carnivalBlocksRepository.GetByIdAsync(carnivalBlockId)
            ?? throw new KeyNotFoundException("Carnival block does not exist.");

        if (carnivalBlock.OwnerId == memberId)
        {
            return RolesEnum.Owner;
        }

        return await _carnivalBlockMembersRepository.GetMemberRole(carnivalBlockId, memberId);
    }
}
