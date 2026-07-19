using BlocoNaRua.Data.Repositories.Interfaces;
using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Domain.Enums;
using BlocoNaRua.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace BlocoNaRua.Services.Implementations;

public class MeetingPresenceService
(
    IMeetingPresencesRepository repository,
    IAuthorizationService authorizationService,
    IMemoryCache cache
) : IMeetingPresenceService
{
    private readonly IMeetingPresencesRepository _repository = repository;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly IMemoryCache _cache = cache;

    public async Task<IList<MeetingPresenceEntity>> GetAllAsync(int? page = null, int? pageSize = null)
    {
        const string cacheKey = "MeetingPresences_All";
        if (!_cache.TryGetValue(cacheKey, out IList<MeetingPresenceEntity>? presences))
        {
            presences = await _repository.GetAllAsync();
            _cache.Set(cacheKey, presences, TimeSpan.FromMinutes(5));
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
        var cacheKey = $"MeetingPresence_{id}";
        if (_cache.TryGetValue(cacheKey, out MeetingPresenceEntity? presence))
        {
            return presence;
        }

        presence = await _repository.GetByIdAsync(id);
        if (presence != null)
        {
            _cache.Set(cacheKey, presence, TimeSpan.FromMinutes(5));
        }
        return presence;
    }

    public async Task<MeetingPresenceEntity> CreateAsync(MeetingPresenceEntity model, int loggedMember)
    {

        if (model.MemberId == loggedMember)
        {
            var created = await _repository.AddAsync(model);
            _cache.Remove("MeetingPresences_All");
            return created;
        }

        var memberRole = await _authorizationService.GetMemberRole(model.CarnivalBlockId, loggedMember);
        if (memberRole == RolesEnum.Owner || memberRole == RolesEnum.Manager)
        {
            var created = await _repository.AddAsync(model);
            _cache.Remove("MeetingPresences_All");
            return created;
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
            _cache.Remove($"MeetingPresence_{id}");
            _cache.Remove("MeetingPresences_All");
            return entity;
        }

        var memberRole = await _authorizationService.GetMemberRole(entity.CarnivalBlockId, loggedMember);
        if (memberRole == RolesEnum.Owner || memberRole == RolesEnum.Manager)
        {
            entity.IsPresent = model.IsPresent;
            await _repository.UpdateAsync(entity);
            _cache.Remove($"MeetingPresence_{id}");
            _cache.Remove("MeetingPresences_All");
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
            var deleted = await _repository.DeleteAsync(entity);
            if (deleted)
            {
                _cache.Remove($"MeetingPresence_{id}");
                _cache.Remove("MeetingPresences_All");
            }
            return deleted;
        }

        var memberRole = await _authorizationService.GetMemberRole(entity.CarnivalBlockId, loggedMember);
        if (memberRole == RolesEnum.Owner || memberRole == RolesEnum.Manager)
        {
            var deleted = await _repository.DeleteAsync(entity);
            if (deleted)
            {
                _cache.Remove($"MeetingPresence_{id}");
                _cache.Remove("MeetingPresences_All");
            }
            return deleted;
        }

        throw new UnauthorizedAccessException("You are not authorized to delete this meeting presence.");
    }

}
