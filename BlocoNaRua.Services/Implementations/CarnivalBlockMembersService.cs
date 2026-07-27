using BlocoNaRua.Data.Repositories.Interfaces;
using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Domain.Enums;
using BlocoNaRua.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace BlocoNaRua.Services.Implementations;

public class CarnivalBlockMembersService
(
    ICarnivalBlockMembersRepository repository,
    IMembersRepository membersRepository,
    ICarnivalBlocksRepository carnivalBlocksRepository,
    IMemoryCache cache
) : ICarnivalBlockMembersService
{
    private readonly ICarnivalBlockMembersRepository _repository = repository;
    private readonly IMembersRepository _membersRepository = membersRepository;
    private readonly ICarnivalBlocksRepository _carnivalBlocksRepository = carnivalBlocksRepository;
    private readonly IMemoryCache _cache = cache;

    private async Task<RolesEnum?> GetMemberRoleInline(int carnivalBlockId, int memberId)
    {
        var carnivalBlock = await _carnivalBlocksRepository.GetByIdAsync(carnivalBlockId, CancellationToken.None)
            ?? throw new KeyNotFoundException("Carnival block does not exist.");
        if (carnivalBlock.OwnerId == memberId)
            return RolesEnum.Owner;
        return await _repository.GetMemberRole(carnivalBlockId, memberId, CancellationToken.None);
    }

    public async Task<List<CarnivalBlockMembersEntity>> GetAllAsync()
    {
        return (await _repository.GetAllAsync(CancellationToken.None)).ToList();
    }

    public async Task<IList<CarnivalBlockMembersEntity>> GetByBlockIdAsync(int blockId)
    {
        return await _repository.GetByBlockIdAsync(blockId, CancellationToken.None);
    }

    public async Task<IList<CarnivalBlockMembersEntity>> GetByMemberIdAsync(int memberId)
    {
        var cacheKey = $"CarnivalBlockMembers_Member_{memberId}";
        if (_cache.TryGetValue(cacheKey, out IList<CarnivalBlockMembersEntity>? carnivalBlockMembers))
        {
            return carnivalBlockMembers!;
        }

        carnivalBlockMembers = await _repository.GetByMemberIdAsync(memberId, CancellationToken.None);
        if (carnivalBlockMembers != null)
        {
            _cache.Set(cacheKey, carnivalBlockMembers, TimeSpan.FromMinutes(5)); // Cache por 5 minutos
        }
        return carnivalBlockMembers!;
    }

    public async Task CreateAsync(CarnivalBlockMembersEntity carnivalBlockMember, int loggedMemberId)
    {
        var member = await _membersRepository.GetByIdAsync(carnivalBlockMember.MemberId, CancellationToken.None)
            ?? throw new KeyNotFoundException("Member does not exist.");

        var carnivalBlock = await _carnivalBlocksRepository.GetByIdAsync(carnivalBlockMember.CarnivalBlockId, CancellationToken.None)
            ?? throw new KeyNotFoundException("Carnival block does not exist.");

        var loggedMemberRole = await GetMemberRoleInline(carnivalBlockMember.CarnivalBlockId, loggedMemberId);

        if (loggedMemberRole != RolesEnum.Owner && loggedMemberRole != RolesEnum.Manager)
        {
            throw new UnauthorizedAccessException("Member is not authorized to add members.");
        }

        var existing = await _repository.GetByBlockIdAsync(carnivalBlockMember.CarnivalBlockId, CancellationToken.None);
        if (existing.Any(m => m.MemberId == carnivalBlockMember.MemberId))
        {
            throw new InvalidOperationException("Member is already part of this carnival block.");
        }

        await _repository.AddAsync(carnivalBlockMember, CancellationToken.None);
        _cache.Remove($"CarnivalBlockMembers_Member_{carnivalBlockMember.MemberId}"); // Invalida o cache
    }

    public async Task<CarnivalBlockMembersEntity?> UpdateAsync(int id, int loggedMemberId, RolesEnum newRole)
    {
        var carnivalBlockMember = await _repository.GetByIdAsync(id, CancellationToken.None)
            ?? throw new KeyNotFoundException("Carnival block member does not exist.");

        var carnivalBlock = await _carnivalBlocksRepository.GetByIdAsync(carnivalBlockMember.CarnivalBlockId, CancellationToken.None)
            ?? throw new KeyNotFoundException("Carnival block does not exist.");

        var loggedMember = await _membersRepository.GetByIdAsync(loggedMemberId, CancellationToken.None)
            ?? throw new KeyNotFoundException("Logged member does not exist.");

        var loggedMemberRole = await GetMemberRoleInline(carnivalBlockMember.CarnivalBlockId, loggedMemberId);

        if (loggedMemberRole != RolesEnum.Owner && loggedMemberRole != RolesEnum.Manager)
        {
            throw new UnauthorizedAccessException("Member is not authorized to update member roles.");
        }

        if (carnivalBlock.OwnerId == carnivalBlockMember.MemberId)
        {
            throw new InvalidOperationException("Cannot change the owner's role.");
        }

        carnivalBlockMember.Role = newRole;
        await _repository.UpdateAsync(id, carnivalBlockMember, CancellationToken.None);
        _cache.Remove($"CarnivalBlockMembers_Member_{carnivalBlockMember.MemberId}"); // Invalida o cache
        return carnivalBlockMember;
    }

    public async Task<bool> DeleteAsync(int id, int loggedMemberId)
    {
        var carnivalBlockMember = await _repository.GetByIdAsync(id, CancellationToken.None)
            ?? throw new KeyNotFoundException("Carnival block member does not exist.");

        var carnivalBlock = await _carnivalBlocksRepository.GetByIdAsync(carnivalBlockMember.CarnivalBlockId, CancellationToken.None)
            ?? throw new KeyNotFoundException("Carnival block does not exist.");

        var loggedMember = await _membersRepository.GetByIdAsync(loggedMemberId, CancellationToken.None)
            ?? throw new KeyNotFoundException("Logged member does not exist.");

        var loggedMemberRole = await GetMemberRoleInline(carnivalBlockMember.CarnivalBlockId, loggedMemberId);

        if (loggedMemberRole != RolesEnum.Owner && loggedMemberRole != RolesEnum.Manager)
        {
            throw new UnauthorizedAccessException("Member is not authorized to remove members.");
        }

        if (carnivalBlock.OwnerId == carnivalBlockMember.MemberId)
        {
            throw new InvalidOperationException("Cannot remove the owner from the carnival block.");
        }

        var deleted = await _repository.DeleteAsync(id, CancellationToken.None);
        if (deleted)
        {
            _cache.Remove($"CarnivalBlockMembers_Member_{carnivalBlockMember.MemberId}"); // Invalida o cache
        }
        return deleted;
    }
}