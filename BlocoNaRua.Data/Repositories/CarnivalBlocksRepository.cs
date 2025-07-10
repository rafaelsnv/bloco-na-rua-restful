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
            ?? throw new ArgumentException("OwnerId does not exist.");

        await _carnivalBlockMembersRepo.AddAsync(new
        (
            id: 0,
            carnivalBlockId: carnivalBlock.Id,
            memberId: owner.Id,
            role: RolesEnum.Owner
        ));

        return await base.AddAsync(carnivalBlock);
    }
}
