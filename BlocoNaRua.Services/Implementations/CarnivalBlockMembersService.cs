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
    IAuthorizationService authorizationService,
    IMemoryCache cache
) : ICarnivalBlockMembersService
{
    private readonly ICarnivalBlockMembersRepository _repository = repository;
    private readonly IMembersRepository _membersRepository = membersRepository;
    private readonly ICarnivalBlocksRepository _carnivalBlocksRepository = carnivalBlocksRepository;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly IMemoryCache _cache = cache;

    public async Task<List<CarnivalBlockMembersEntity>> GetAllAsync()
    {
        return (await _repository.GetAllAsync()).ToList();
    }

    public async Task<IList<CarnivalBlockMembersEntity>> GetByBlockIdAsync(int blockId)
    {
        return await _repository.GetByBlockIdAsync(blockId);
    }

    public async Task<IList<CarnivalBlockMembersEntity>> GetByMemberIdAsync(int memberId)
    {
        var cacheKey = $"CarnivalBlockMembers_Member_{memberId}";
        if (_cache.TryGetValue(cacheKey, out IList<CarnivalBlockMembersEntity>? carnivalBlockMembers))
        {
            return carnivalBlockMembers!;
        }

        carnivalBlockMembers = await _repository.GetByMemberIdAsync(memberId);
        if (carnivalBlockMembers != null)
        {
            _cache.Set(cacheKey, carnivalBlockMembers, TimeSpan.FromMinutes(5)); // Cache por 5 minutos
        }
        return carnivalBlockMembers!;
    }

    public async Task CreateAsync(CarnivalBlockMembersEntity carnivalBlockMember, int loggedMemberId)
    {
        var member = await _membersRepository.GetByIdAsync(carnivalBlockMember.MemberId)
            ?? throw new KeyNotFoundException("Member does not exist.");

        var carnivalBlock = await _carnivalBlocksRepository.GetByIdAsync(carnivalBlockMember.CarnivalBlockId)
            ?? throw new KeyNotFoundException("Carnival block does not exist.");

        var loggedMemberRole = await _authorizationService.GetMemberRole(carnivalBlockMember.CarnivalBlockId, loggedMemberId);

        if (loggedMemberRole != RolesEnum.Owner && loggedMemberRole != RolesEnum.Manager)
        {
            throw new UnauthorizedAccessException("Member is not authorized to add members.");
        }

        await _repository.AddAsync(carnivalBlockMember);
        _cache.Remove($"CarnivalBlockMembers_Member_{carnivalBlockMember.MemberId}"); // Invalida o cache
    }

    public async Task<CarnivalBlockMembersEntity?> UpdateAsync(int id, int loggedMemberId, RolesEnum newRole)
    {
        var carnivalBlockMember = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Carnival block member does not exist.");

        var carnivalBlock = await _carnivalBlocksRepository.GetByIdAsync(carnivalBlockMember.CarnivalBlockId)
            ?? throw new KeyNotFoundException("Carnival block does not exist.");

        var loggedMember = await _membersRepository.GetByIdAsync(loggedMemberId)
            ?? throw new KeyNotFoundException("Logged member does not exist.");

        var loggedMemberRole = await _authorizationService.GetMemberRole(carnivalBlockMember.CarnivalBlockId, loggedMemberId);

        if (loggedMemberRole != RolesEnum.Owner && loggedMemberRole != RolesEnum.Manager)
        {
            throw new UnauthorizedAccessException("Member is not authorized to update member roles.");
        }

        if (carnivalBlock.OwnerId == carnivalBlockMember.MemberId)
        {
            throw new InvalidOperationException("Cannot change the owner's role.");
        }

        carnivalBlockMember.Role = newRole;
        await _repository.UpdateAsync(carnivalBlockMember);
        _cache.Remove($"CarnivalBlockMembers_Member_{carnivalBlockMember.MemberId}"); // Invalida o cache
        return carnivalBlockMember;
    }

    public async Task<bool> DeleteAsync(int id, int loggedMemberId)
    {
        var carnivalBlockMember = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Carnival block member does not exist.");

        var carnivalBlock = await _carnivalBlocksRepository.GetByIdAsync(carnivalBlockMember.CarnivalBlockId)
            ?? throw new KeyNotFoundException("Carnival block does not exist.");

        var loggedMember = await _membersRepository.GetByIdAsync(loggedMemberId)
            ?? throw new KeyNotFoundException("Logged member does not exist.");

        var loggedMemberRole = await _authorizationService.GetMemberRole(carnivalBlockMember.CarnivalBlockId, loggedMemberId);

        if (loggedMemberRole != RolesEnum.Owner && loggedMemberRole != RolesEnum.Manager)
        {
            throw new UnauthorizedAccessException("Member is not authorized to remove members.");
        }

        if (carnivalBlock.OwnerId == carnivalBlockMember.MemberId)
        {
            throw new InvalidOperationException("Cannot remove the owner from the carnival block.");
        }

        var deleted = await _repository.DeleteAsync(carnivalBlockMember);
        if (deleted)
        {
            _cache.Remove($"CarnivalBlockMembers_Member_{carnivalBlockMember.MemberId}"); // Invalida o cache
        }
        return deleted;
    }
}
