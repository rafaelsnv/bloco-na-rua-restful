using BlocoNaRua.Data.Repositories;
using BlocoNaRua.Data.Repositories.Interfaces;
using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace BlocoNaRua.Services.Implementations;

public class MembersService(
    IMembersRepository repository,
    ICarnivalBlockMembersRepository carnivalBlockMembersRepository,
    IMeetingsRepository meetingsRepository,
    IMemoryCache cache) : IMembersService
{
    public async Task<IList<MemberEntity>> GetAllAsync(int? page = null, int? pageSize = null)
    {
        var allMembers = await repository.GetAllAsync(null, null, CancellationToken.None);
        
        if (page.HasValue && pageSize.HasValue)
        {
            var skip = (page.Value - 1) * pageSize.Value;
            return allMembers.Skip(skip).Take(pageSize.Value).ToList();
        }
        
        return allMembers;
    }

    public async Task<MemberEntity?> GetByIdAsync(int id)
    {
        return await repository.GetByIdAsync(id, CancellationToken.None);
    }

    public async Task<MemberEntity?> GetByUuidAsync(Guid uuid)
    {
        if (cache.TryGetValue($"Member_{uuid}", out MemberEntity? member))
        {
            return member;
        }

        member = await repository.GetByUuidAsync(uuid, CancellationToken.None);
        if (member != null)
        {
            cache.Set($"Member_{uuid}", member, TimeSpan.FromMinutes(5)); // Cache por 5 minutos
        }
        return member;
    }

    public async Task<IList<CarnivalBlockMembersEntity>> GetMemberBlocksAsync(int memberId)
    {
        return await carnivalBlockMembersRepository.GetByMemberIdAsync(memberId, CancellationToken.None);
    }

    public async Task<IList<MeetingEntity>> GetMemberMeetingsAsync(int memberId)
    {
        var memberBlocks = await carnivalBlockMembersRepository.GetByMemberIdAsync(memberId, CancellationToken.None);
        if (memberBlocks == null || !memberBlocks.Any())
            return new List<MeetingEntity>();

        var blockIds = memberBlocks.Select(mb => mb.CarnivalBlockId).ToList();

        return await meetingsRepository.GetByBlockIdsAsync(blockIds, CancellationToken.None);
    }

    public async Task<MemberEntity> CreateAsync(MemberEntity entity)
    {
        var newMember = new MemberEntity(
            0,
            entity.Name,
            entity.Email,
            entity.Phone,
            entity.ProfileImage,
            entity.Uuid
        );
        var createdMember = await repository.AddAsync(newMember, CancellationToken.None);
        if (createdMember != null)
        {
            cache.Remove($"Member_{createdMember.Uuid}"); // Invalida o cache após a criação
        }
        return createdMember!;
    }

    public async Task<MemberEntity?> UpdateAsync(int id, int loggedMember, MemberEntity model)
    {
        if (id != loggedMember)
            throw new UnauthorizedAccessException("Member is not authorized to update this resource.");

        var entity = await repository.GetByIdAsync(id, CancellationToken.None);
        if (entity is null)
            return null;

        // Update properties based on the model
        entity.Name = model.Name;
        entity.Email = model.Email;
        entity.Phone = model.Phone;
        entity.ProfileImage = model.ProfileImage;

        await repository.UpdateAsync(entity, CancellationToken.None);
        cache.Remove($"Member_{entity.Uuid}"); // Invalida o cache após a atualização
        return entity;
    }

    public async Task<bool> DeleteAsync(int id, int loggedMember)
    {
        if (id != loggedMember)
            throw new UnauthorizedAccessException("Member is not authorized to delete this resource.");

        var entity = await repository.GetByIdAsync(id, CancellationToken.None);
        if (entity is null)
            return false;
        var deleted = await repository.DeleteAsync(entity, CancellationToken.None);
        if (deleted)
        {
            cache.Remove($"Member_{entity.Uuid}"); // Invalida o cache após a exclusão
        }
        return deleted;
    }
}

