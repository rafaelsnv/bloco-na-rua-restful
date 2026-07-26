using BlocoNaRua.Data.Repositories.Interfaces;
using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Domain.Enums;
using BlocoNaRua.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace BlocoNaRua.Services.Implementations;

public class MeetingPresenceService
(
    IMeetingPresencesRepository repository,
    ICarnivalBlocksRepository carnivalBlocksRepository,
    ICarnivalBlockMembersRepository carnivalBlockMembersRepository,
    IMemoryCache cache
) : IMeetingPresenceService
{
    private readonly IMeetingPresencesRepository _repository = repository;
    private readonly ICarnivalBlocksRepository _carnivalBlocksRepository = carnivalBlocksRepository;
    private readonly ICarnivalBlockMembersRepository _carnivalBlockMembersRepository = carnivalBlockMembersRepository;
    private readonly IMemoryCache _cache = cache;

    private async Task<RolesEnum?> GetMemberRoleInline(int carnivalBlockId, int memberId)
    {
        var carnivalBlock = await _carnivalBlocksRepository.GetByIdAsync(carnivalBlockId, CancellationToken.None)
            ?? throw new KeyNotFoundException("Carnival block does not exist.");
        if (carnivalBlock.OwnerId == memberId)
            return RolesEnum.Owner;
        return await _carnivalBlockMembersRepository.GetMemberRole(carnivalBlockId, memberId, CancellationToken.None);
    }

    private async Task AuthorizeForMemberAsync(int carnivalBlockId, int loggedMemberId, int targetMemberId)
    {
        if (loggedMemberId == targetMemberId) return;

        var memberRole = await GetMemberRoleInline(carnivalBlockId, loggedMemberId);
        if (memberRole == RolesEnum.Owner || memberRole == RolesEnum.Manager) return;

        throw new UnauthorizedAccessException("You are not authorized to access this meeting presence for another member.");
    }

    public async Task<IList<MeetingPresenceEntity>> GetAllAsync(int? page = null, int? pageSize = null)
    {
        if (!_cache.TryGetValue("MeetingPresences_All", out IList<MeetingPresenceEntity>? presences))
        {
            presences = await _repository.GetAllAsync(CancellationToken.None);
            _cache.Set("MeetingPresences_All", presences, TimeSpan.FromMinutes(5));
        }

        if (page.HasValue && pageSize.HasValue)
        {
            var skip = (page.Value - 1) * pageSize.Value;
            return presences!.Skip(skip).Take(pageSize.Value).ToList();
        }

        return presences!;
    }

    public async Task<MeetingPresenceEntity?> GetByIdAsync(int id)
    {
        if (_cache.TryGetValue($"MeetingPresence_{id}", out MeetingPresenceEntity? presence))
        {
            return presence;
        }

        presence = await _repository.GetByIdAsync(id, CancellationToken.None);
        if (presence != null)
        {
            _cache.Set($"MeetingPresence_{id}", presence, TimeSpan.FromMinutes(5));
        }
        return presence;
    }

    public async Task<MeetingPresenceEntity> CreateAsync(MeetingPresenceEntity model, int loggedMember)
    {
        await AuthorizeForMemberAsync(model.CarnivalBlockId, loggedMember, model.MemberId);

        var created = await _repository.AddAsync(model, CancellationToken.None);
        _cache.Remove("MeetingPresences_All");
        return created;
    }

    public async Task<MeetingPresenceEntity?> UpdateAsync(int id, MeetingPresenceEntity model, int loggedMember)
    {
        var entity = await _repository.GetByIdAsync(id, CancellationToken.None)
            ?? throw new KeyNotFoundException("Meeting presence does not exist.");

        await AuthorizeForMemberAsync(entity.CarnivalBlockId, loggedMember, entity.MemberId);

        entity.IsPresent = model.IsPresent;
        await _repository.UpdateAsync(id, entity, CancellationToken.None);
        _cache.Remove($"MeetingPresence_{id}");
        _cache.Remove("MeetingPresences_All");
        return entity;
    }

    public async Task<bool> DeleteAsync(int id, int loggedMember)
    {
        var entity = await _repository.GetByIdAsync(id, CancellationToken.None)
            ?? throw new KeyNotFoundException("Meeting presence does not exist.");

        await AuthorizeForMemberAsync(entity.CarnivalBlockId, loggedMember, entity.MemberId);

        var deleted = await _repository.DeleteAsync(id, CancellationToken.None);
        if (deleted)
        {
            _cache.Remove($"MeetingPresence_{id}");
            _cache.Remove("MeetingPresences_All");
        }
        return deleted;
    }

}