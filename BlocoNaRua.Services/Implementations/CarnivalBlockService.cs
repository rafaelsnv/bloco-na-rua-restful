using BlocoNaRua.Data.Repositories.Interfaces;
using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Domain.Enums;
using BlocoNaRua.Services.Interfaces;

namespace BlocoNaRua.Services.Implementations;

public class CarnivalBlockService
(
    ICarnivalBlocksRepository repository,
    ICarnivalBlockMembersRepository carnivalBlockMembersRepository,
    IMembersRepository membersRepository,
    IAuthorizationService authorizationService
) : ICarnivalBlockService
{
    private readonly ICarnivalBlocksRepository _repository = repository;
    private readonly ICarnivalBlockMembersRepository _carnivalBlockMembersRepository = carnivalBlockMembersRepository;
    private readonly IMembersRepository _membersRepository = membersRepository;
    private readonly IAuthorizationService _authorizationService = authorizationService;

    public async Task<IList<CarnivalBlockEntity>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<CarnivalBlockEntity?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
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
        return await _repository.AddAsync(entity);
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

        return await _repository.DeleteAsync(entity);
    }

    private static string GenerateInviteCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 8)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
