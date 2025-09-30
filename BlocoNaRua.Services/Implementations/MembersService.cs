using BlocoNaRua.Data.Repositories.Interfaces;
using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace BlocoNaRua.Services.Implementations;

public class MembersService(IMembersRepository repository, IMemoryCache cache) : IMembersService
{
    private readonly IMembersRepository _repository = repository;
    private readonly IMemoryCache _cache = cache;

    public async Task<IList<MemberEntity>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<MemberEntity?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<MemberEntity?> GetByUuidAsync(Guid uuid)
    {
        var cacheKey = $"Member_{uuid}";
        if (_cache.TryGetValue(cacheKey, out MemberEntity? member))
        {
            return member;
        }

        member = await _repository.GetByUuidAsync(uuid);
        if (member != null)
        {
            _cache.Set(cacheKey, member, TimeSpan.FromMinutes(5)); // Cache por 5 minutos
        }
        return member;
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
        var createdMember = await _repository.AddAsync(newMember);
        if (createdMember != null)
        {
            _cache.Remove($"Member_{createdMember.Uuid}"); // Invalida o cache após a criação
        }
        return createdMember;
    }

    public async Task<MemberEntity?> UpdateAsync(int id, int loggedMember, MemberEntity model)
    {
        if (id != loggedMember)
            throw new UnauthorizedAccessException("Member is not authorized to update this resource.");

        var entity = await _repository.GetByIdAsync(id);
        if (entity is null)
            return null;

        // Update properties based on the model
        entity.Name = model.Name;
        entity.Email = model.Email;
        entity.Phone = model.Phone;
        entity.ProfileImage = model.ProfileImage;

        await _repository.UpdateAsync(entity);
        _cache.Remove($"Member_{entity.Uuid}"); // Invalida o cache após a atualização
        return entity;
    }

    public async Task<bool> DeleteAsync(int id, int loggedMember)
    {
        if (id != loggedMember)
            throw new UnauthorizedAccessException("Member is not authorized to delete this resource.");

        var entity = await _repository.GetByIdAsync(id);
        if (entity is null)
            return false;
        var deleted = await _repository.DeleteAsync(entity);
        if (deleted)
        {
            _cache.Remove($"Member_{entity.Uuid}"); // Invalida o cache após a exclusão
        }
        return deleted;
    }
}
