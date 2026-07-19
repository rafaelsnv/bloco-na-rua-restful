using BlocoNaRua.Data.Repositories.Interfaces;
using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Domain.Enums;
using BlocoNaRua.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace BlocoNaRua.Services.Implementations;

public class CarnivalBlockService
(
    ICarnivalBlocksRepository repository,
    IMembersRepository membersRepository,
    IAuthorizationService authorizationService,
    IMemoryCache cache
) : ICarnivalBlockService
{
    private readonly ICarnivalBlocksRepository _repository = repository;
    private readonly IMembersRepository _membersRepository = membersRepository;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly IMemoryCache _cache = cache;

    public async Task<IList<CarnivalBlockEntity>> GetAllAsync(int? page = null, int? pageSize = null)
    {
        const string cacheKey = "CarnivalBlocks_All";
        if (!_cache.TryGetValue(cacheKey, out IList<CarnivalBlockEntity>? blocks))
        {
            blocks = await _repository.GetAllAsync();
            _cache.Set(cacheKey, blocks, TimeSpan.FromMinutes(5));
        }

        if (page.HasValue && pageSize.HasValue)
        {
            var skip = (page.Value - 1) * pageSize.Value;
            return blocks!.Skip(skip).Take(pageSize.Value).ToList();
        }

        return blocks!;
    }

    public async Task<CarnivalBlockEntity?> GetByIdAsync(int id)
    {
        var cacheKey = $"CarnivalBlock_{id}";
        if (_cache.TryGetValue(cacheKey, out CarnivalBlockEntity? block))
        {
            return block;
        }

        block = await _repository.GetByIdAsync(id);
        if (block != null)
        {
            _cache.Set(cacheKey, block, TimeSpan.FromMinutes(5));
        }
        return block;
    }

    public async Task<CarnivalBlockEntity> CreateAsync(CarnivalBlockEntity model)
    {
        var owner = await _membersRepository.GetByIdAsync(model.OwnerId)
            ?? throw new KeyNotFoundException("Member does not exist.");

        var entity = new CarnivalBlockEntity
        (
            0,
            model.OwnerId,
            model.Name,
            GenerateInviteCode(),
            GenerateInviteCode(),
            model.CarnivalBlockImage
        );
        var created = await _repository.AddAsync(entity);
        _cache.Remove("CarnivalBlocks_All");
        return created;
    }

    public async Task<CarnivalBlockEntity?> UpdateAsync(int id, int loggedMember, CarnivalBlockEntity model)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Carnival block does not exist.");

        var member = await _membersRepository.GetByIdAsync(loggedMember)
            ?? throw new KeyNotFoundException("Member does not exist.");

        var memberRole = await _authorizationService.GetMemberRole(id, loggedMember);

        if (memberRole != RolesEnum.Owner && memberRole != RolesEnum.Manager)
        {
            throw new UnauthorizedAccessException("Member is not authorized to update this carnival block.");
        }

        entity.Name = model.Name;
        entity.CarnivalBlockImage = model.CarnivalBlockImage;
        await _repository.UpdateAsync(entity);
        _cache.Remove($"CarnivalBlock_{id}");
        _cache.Remove("CarnivalBlocks_All");
        return entity;
    }

    public async Task<bool> DeleteAsync(int id, int loggedMember)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Carnival block does not exist.");

        var member = await _membersRepository.GetByIdAsync(loggedMember)
            ?? throw new KeyNotFoundException("Member does not exist.");

        var memberRole = await _authorizationService.GetMemberRole(id, loggedMember);

        if (memberRole != RolesEnum.Owner)
        {
            throw new UnauthorizedAccessException("Member is not authorized to delete this carnival block.");
        }

        var deleted = await _repository.DeleteAsync(entity);
        if (deleted)
        {
            _cache.Remove($"CarnivalBlock_{id}");
            _cache.Remove("CarnivalBlocks_All");
        }
        return deleted;
    }

    private static string GenerateInviteCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, 8)
            .Select(s => s[Random.Shared.Next(s.Length)]).ToArray());
    }
}
