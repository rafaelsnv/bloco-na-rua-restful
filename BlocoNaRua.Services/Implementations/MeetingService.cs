using BlocoNaRua.Data.Repositories.Interfaces;
using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Domain.Enums;
using BlocoNaRua.Services.Interfaces;

namespace BlocoNaRua.Services.Implementations;

public class MeetingService
(
    IMeetingsRepository repository,
    ICarnivalBlocksRepository carnivalBlocksRepository,
    ICarnivalBlockMembersRepository carnivalBlockMembersRepository
) : IMeetingService
{
    private readonly IMeetingsRepository _repository = repository;
    private readonly ICarnivalBlocksRepository _carnivalBlocksRepository = carnivalBlocksRepository;
    private readonly ICarnivalBlockMembersRepository _carnivalBlockMembersRepository = carnivalBlockMembersRepository;

    public async Task<IList<MeetingEntity>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<MeetingEntity?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<MeetingEntity> CreateAsync(MeetingEntity model, int loggedMember)
    {
        var memberRole = await GetMemberRole(model.CarnivalBlockId, loggedMember);

        if (memberRole != RolesEnum.Owner && memberRole != RolesEnum.Manager)
        {
            throw new UnauthorizedAccessException("Member is not authorized to create a meeting for this carnival block.");
        }

        var entity = new MeetingEntity
        (
            0,
            model.Name,
            model.Description,
            model.Location,
            GenerateMeetingCode(),
            model.MeetingDateTime,
            model.CarnivalBlockId
        );
        return await _repository.AddAsync(entity);
    }

    public async Task<MeetingEntity?> UpdateAsync(int id, MeetingEntity model, int loggedMember)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Meeting does not exist.");

        var memberRole = await GetMemberRole(entity.CarnivalBlockId, loggedMember);

        if (memberRole != RolesEnum.Owner && memberRole != RolesEnum.Manager)
        {
            throw new UnauthorizedAccessException("Member is not authorized to update this meeting.");
        }

        entity.Name = model.Name;
        entity.Description = model.Description;
        entity.Location = model.Location;
        entity.MeetingDateTime = model.MeetingDateTime;

        await _repository.UpdateAsync(entity);
        return entity;
    }

    public async Task<bool> DeleteAsync(int id, int loggedMember)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Meeting does not exist.");

        var memberRole = await GetMemberRole(entity.CarnivalBlockId, loggedMember);

        if (memberRole != RolesEnum.Owner && memberRole != RolesEnum.Manager)
        {
            throw new UnauthorizedAccessException("Member is not authorized to delete this meeting.");
        }

        return await _repository.DeleteAsync(entity);
    }

    private async Task<RolesEnum?> GetMemberRole(int carnivalBlockId, int loggedMember)
    {
        var carnivalBlock = await _carnivalBlocksRepository.GetByIdAsync(carnivalBlockId)
            ?? throw new KeyNotFoundException("Carnival block does not exist.");

        if (carnivalBlock.OwnerId == loggedMember)
        {
            return RolesEnum.Owner;
        }

        return await _carnivalBlockMembersRepository.GetMemberRole(carnivalBlockId, loggedMember);
    }

    private static string GenerateMeetingCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 6)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
