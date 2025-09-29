using BlocoNaRua.Data.Context;
using BlocoNaRua.Data.Repositories.Base;
using BlocoNaRua.Data.Repositories.Interfaces;
using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BlocoNaRua.Data.Repositories;

public class CarnivalBlockMembersRepository
(
    AppDbContext appContext
) : RepositoryBase<CarnivalBlockMembersEntity>(appContext), ICarnivalBlockMembersRepository
{
    public async Task<RolesEnum?> GetMemberRole(int carnivalBlockId, int memberId)
    {
        var carnivalBlockMember = await DbSet.AsNoTracking().FirstOrDefaultAsync
            (cbMember =>
                cbMember.CarnivalBlockId == carnivalBlockId &&
                cbMember.MemberId == memberId
            );

        return carnivalBlockMember?.Role;
    }

    public async Task<IList<CarnivalBlockMembersEntity>> GetByBlockIdAsync(int blockId)
    {
        return await DbSet
            .AsNoTracking()
            .Where(cbMember => cbMember.CarnivalBlockId == blockId)
            .ToListAsync();
    }

    public async Task<IList<CarnivalBlockMembersEntity>> GetByMemberIdAsync(int memberId)
    {
        return await DbSet
            .AsNoTracking()
            .Include(cbMember => cbMember.CarnivalBlock)
            .Where(cbMember => cbMember.MemberId == memberId)
            .ToListAsync();
    }
}
