using BlocoNaRua.Data.Repositories.Interfaces;
using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Domain.Enums;
using BlocoNaRua.Services.Interfaces;
using BlocoNaRua.Services.Utils;
using Microsoft.Extensions.Caching.Memory;

namespace BlocoNaRua.Services.Implementations;

public class CarnivalBlockService
(
    ICarnivalBlocksRepository repository,
    IMembersRepository membersRepository,
    ICarnivalBlockMembersRepository carnivalBlockMembersRepository,
    IMemoryCache cache
) : ICarnivalBlockService
{
    private readonly ICarnivalBlocksRepository _repository = repository;
    private readonly IMembersRepository _membersRepository = membersRepository;
    private readonly ICarnivalBlockMembersRepository _carnivalBlockMembersRepository = carnivalBlockMembersRepository;
    private readonly IMemoryCache _cache = cache;

    private async Task<RolesEnum?> GetMemberRoleInline(int carnivalBlockId, int memberId)
    {
        var carnivalBlock = await _repository.GetByIdAsync(carnivalBlockId, CancellationToken.None)
            ?? throw new KeyNotFoundException("Carnival block does not exist.");
        if (carnivalBlock.OwnerId == memberId)
            return RolesEnum.Owner;
        return await _carnivalBlockMembersRepository.GetMemberRole(carnivalBlockId, memberId, CancellationToken.None);
    }

    public async Task<IList<CarnivalBlockEntity>> GetAllAsync(int? page = null, int? pageSize = null)
    {
        if (!_cache.TryGetValue("CarnivalBlocks_All", out IList<CarnivalBlockEntity>? blocks))
        {
            blocks = await _repository.GetAllAsync(CancellationToken.None);
            _cache.Set("CarnivalBlocks_All", blocks, TimeSpan.FromMinutes(5));
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
        if (_cache.TryGetValue($"CarnivalBlock_{id}", out CarnivalBlockEntity? block))
        {
            return block;
        }

        block = await _repository.GetByIdAsync(id, CancellationToken.None);
        if (block != null)
        {
            _cache.Set($"CarnivalBlock_{id}", block, TimeSpan.FromMinutes(5));
        }
        return block;
    }

    public async Task<CarnivalBlockEntity> CreateAsync(CarnivalBlockEntity model)
    {
        var owner = await _membersRepository.GetByIdAsync(model.OwnerId, CancellationToken.None)
            ?? throw new KeyNotFoundException("Member does not exist.");

        var entity = new CarnivalBlockEntity
        (
            0,
            model.OwnerId,
            model.Name,
            CodeGenerator.Generate(),
            CodeGenerator.Generate(),
            model.CarnivalBlockImage
        );
        var created = await _repository.AddAsync(entity, CancellationToken.None);

        var ownerMember = new CarnivalBlockMembersEntity(0, created.Id, model.OwnerId, RolesEnum.Owner);
        await _carnivalBlockMembersRepository.AddAsync(ownerMember, CancellationToken.None);

        _cache.Remove("CarnivalBlocks_All");
        return created;
    }

    public async Task<CarnivalBlockEntity?> UpdateAsync(int id, int loggedMember, CarnivalBlockEntity model)
    {
        var entity = await _repository.GetByIdAsync(id, CancellationToken.None)
            ?? throw new KeyNotFoundException("Carnival block does not exist.");

        var member = await _membersRepository.GetByIdAsync(loggedMember, CancellationToken.None)
            ?? throw new KeyNotFoundException("Member does not exist.");

        var memberRole = await GetMemberRoleInline(id, loggedMember);

        if (memberRole != RolesEnum.Owner && memberRole != RolesEnum.Manager)
        {
            throw new UnauthorizedAccessException("Member is not authorized to update this carnival block.");
        }

        entity.Name = model.Name;
        entity.CarnivalBlockImage = model.CarnivalBlockImage;
        await _repository.UpdateAsync(id, entity, CancellationToken.None);
        _cache.Remove($"CarnivalBlock_{id}");
        _cache.Remove("CarnivalBlocks_All");
        return entity;
    }

    public async Task<bool> DeleteAsync(int id, int loggedMember)
    {
        var entity = await _repository.GetByIdAsync(id, CancellationToken.None)
            ?? throw new KeyNotFoundException("Carnival block does not exist.");

        var member = await _membersRepository.GetByIdAsync(loggedMember, CancellationToken.None)
            ?? throw new KeyNotFoundException("Member does not exist.");

        var memberRole = await GetMemberRoleInline(id, loggedMember);

        if (memberRole != RolesEnum.Owner)
        {
            throw new UnauthorizedAccessException("Member is not authorized to delete this carnival block.");
        }

        var deleted = await _repository.DeleteAsync(id, CancellationToken.None);
        if (deleted)
        {
            _cache.Remove($"CarnivalBlock_{id}");
            _cache.Remove("CarnivalBlocks_All");
        }
        return deleted;
    }
}