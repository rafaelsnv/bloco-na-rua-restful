using BlocoNaRua.Data.Context;
using BlocoNaRua.Data.Repositories.Base;
using BlocoNaRua.Data.Repositories.Interfaces;
using BlocoNaRua.Domain.Entities;

namespace BlocoNaRua.Data.Repositories;

public class CarnivalBlockMembersRepository
(
    AppDbContext appContext,
    IMembersRepository membersRepo,
    ICarnivalBlocksRepository carnivalBlocksRepo
) : RepositoryBase<CarnivalBlockMembersEntity>(appContext), ICarnivalBlockMembersRepository
{
    private readonly IMembersRepository _membersRepo = membersRepo;
    private readonly ICarnivalBlocksRepository _carnivalBlocksRepo = carnivalBlocksRepo;
    new public async Task<CarnivalBlockMembersEntity> AddAsync(CarnivalBlockMembersEntity blockMember)
    {
        var member = await _membersRepo.GetByIdAsync(blockMember.MemberId)
            ?? throw new KeyNotFoundException("Member does not exist.");
        var carnivalBlock = await _carnivalBlocksRepo.GetByIdAsync(blockMember.CarnivalBlockId)
            ?? throw new KeyNotFoundException("Carnival block does not exist.");
        return await base.AddAsync(blockMember);
    }
}
