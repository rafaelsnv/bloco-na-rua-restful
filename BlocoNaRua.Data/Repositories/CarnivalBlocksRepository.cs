using BlocoNaRua.Data.Context;
using BlocoNaRua.Data.Repositories.Base;
using BlocoNaRua.Data.Repositories.Interfaces;
using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Domain.Enums;

namespace BlocoNaRua.Data.Repositories;

public class CarnivalBlocksRepository(AppDbContext appContext, IMembersRepository membersRepository,
ICarnivalBlockMembersRepository carnivalBlockMembersRepo) : RepositoryBase<CarnivalBlockEntity>(appContext), ICarnivalBlocksRepository
{
    private readonly IMembersRepository _membersRepository = membersRepository;
    private readonly ICarnivalBlockMembersRepository _carnivalBlockMembersRepo = carnivalBlockMembersRepo;
    new public async Task<CarnivalBlockEntity> AddAsync(CarnivalBlockEntity carnivalBlock)
    {
        var owner = await _membersRepository.GetByIdAsync(carnivalBlock.OwnerId)
            ?? throw new KeyNotFoundException("Member does not exist.");

        await _carnivalBlockMembersRepo.AddAsync(new
        (
            id: 0,
            carnivalBlockId: carnivalBlock.Id,
            memberId: owner.Id,
            role: RolesEnum.Owner
        ));

        return await base.AddAsync(carnivalBlock);
    }

    public async Task<bool> UpdateAsync(int memberId, CarnivalBlockEntity carnivalBlock)
    {
        var existingCarnivalBlock = await GetByIdAsync(carnivalBlock.Id)
            ?? throw new KeyNotFoundException("Carnival block does not exist.");

        if (existingCarnivalBlock.OwnerId != memberId)
        {
            var member = await _carnivalBlockMembersRepo.GetByIdAsync(memberId)
            ?? throw new KeyNotFoundException("Member does not exist.");

            if (member.Role == RolesEnum.Member)
                throw new UnauthorizedAccessException
                (
                    "Insufficient permissions to modify this carnival block."
                );
        }

        return await UpdateAsync(carnivalBlock);
    }
}
