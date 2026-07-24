using BlocoNaRua.Data.Repositories.Interfaces;
using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Domain.Enums;
using BlocoNaRua.Services.Interfaces;
using BlocoNaRua.Services.Utils;
using Microsoft.Extensions.Caching.Memory;

namespace BlocoNaRua.Services.Implementations;

public class MeetingService
(
    IMeetingsRepository repository,
    IAuthorizationService authorizationService,
    IMemoryCache cache
) : IMeetingService
{
    private readonly IMeetingsRepository _repository = repository;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly IMemoryCache _cache = cache;

    public async Task<IList<MeetingEntity>> GetAllAsync(int? page = null, int? pageSize = null)
    {
        const string cacheKey = "Meetings_All";
        if (!_cache.TryGetValue(cacheKey, out IList<MeetingEntity>? meetings))
        {
            meetings = await _repository.GetAllAsync();
            _cache.Set(cacheKey, meetings, TimeSpan.FromMinutes(5));
        }

        if (page.HasValue && pageSize.HasValue)
        {
            var skip = (page.Value - 1) * pageSize.Value;
            return meetings!.Skip(skip).Take(pageSize.Value).ToList();
        }

        return meetings!;
    }

    public async Task<MeetingEntity?> GetByIdAsync(int id)
    {
        var cacheKey = $"Meeting_{id}";
        if (_cache.TryGetValue(cacheKey, out MeetingEntity? meeting))
        {
            return meeting;
        }

        meeting = await _repository.GetByIdAsync(id);
        if (meeting != null)
        {
            _cache.Set(cacheKey, meeting, TimeSpan.FromMinutes(5));
        }
        return meeting;
    }

    public async Task<IList<MeetingEntity>> GetAllByBlockIdAsync(int blockId)
    {
        return await _repository.GetAllByBlockIdAsync(blockId);
    }

    public async Task<MeetingEntity> CreateAsync(MeetingEntity model, int loggedMember)
    {
        var memberRole = await _authorizationService.GetMemberRole(model.CarnivalBlockId, loggedMember);

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
            CodeGenerator.Generate(6),
            model.MeetingDateTime,
            model.CarnivalBlockId
        );
        var created = await _repository.AddAsync(entity);
        _cache.Remove("Meetings_All");
        return created;
    }

    public async Task<MeetingEntity?> UpdateAsync(int id, MeetingEntity model, int loggedMember)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Meeting does not exist.");

        var memberRole = await _authorizationService.GetMemberRole(entity.CarnivalBlockId, loggedMember);

        if (memberRole != RolesEnum.Owner && memberRole != RolesEnum.Manager)
        {
            throw new UnauthorizedAccessException("Member is not authorized to update this meeting.");
        }

        entity.Name = model.Name;
        entity.Description = model.Description;
        entity.Location = model.Location;
        entity.MeetingDateTime = model.MeetingDateTime;

        await _repository.UpdateAsync(entity);
        _cache.Remove($"Meeting_{id}");
        _cache.Remove("Meetings_All");
        return entity;
    }

    public async Task<bool> DeleteAsync(int id, int loggedMember)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Meeting does not exist.");

        var memberRole = await _authorizationService.GetMemberRole(entity.CarnivalBlockId, loggedMember);

        if (memberRole != RolesEnum.Owner && memberRole != RolesEnum.Manager)
        {
            throw new UnauthorizedAccessException("Member is not authorized to delete this meeting.");
        }

        var deleted = await _repository.DeleteAsync(entity);
        if (deleted)
        {
            _cache.Remove($"Meeting_{id}");
            _cache.Remove("Meetings_All");
        }
        return deleted;
    }
}
