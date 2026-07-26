using BlocoNaRua.Data.Context;
using BlocoNaRua.Data.Repositories.Base;
using BlocoNaRua.Data.Repositories.Interfaces;
using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BlocoNaRua.Data.Repositories;

public class CarnivalBlockMembersRepository(AppDbContext appContext) : RepositoryBase<CarnivalBlockMembersEntity>(appContext), ICarnivalBlockMembersRepository
{
    public async Task<IList<CarnivalBlockMembersEntity>> GetByMemberIdAsync(int memberId, CancellationToken ct)
    {
        return await DbSet
            .AsNoTracking()
            .Include(cbMember => cbMember.CarnivalBlock)
            .Where(cbMember => cbMember.MemberId == memberId)
            .ToListAsync(ct);
    }

    public async Task<CarnivalBlockMembersEntity?> GetByMemberAndBlockAsync(int carnivalBlockId, int memberId, CancellationToken ct)
    {
        return await DbSet.AsNoTracking().FirstOrDefaultAsync(
            cb => cb.CarnivalBlockId == carnivalBlockId && cb.MemberId == memberId, ct);
    }

    public async Task<RolesEnum?> GetMemberRole(int carnivalBlockId, int memberId, CancellationToken ct)
    {
        var carnivalBlockMember = await DbSet.AsNoTracking().FirstOrDefaultAsync
            (cbMember =>
                cbMember.CarnivalBlockId == carnivalBlockId &&
                cbMember.MemberId == memberId
            , ct);

        return carnivalBlockMember?.Role;
    }

    public async Task<CarnivalBlockMembersEntity> AddAsync(CarnivalBlockMembersEntity entity, CancellationToken ct)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = entity.CreatedAt;
        var result = await DbSet.AddAsync(entity, ct);
        await AppDbContext.SaveChangesAsync(ct);
        return result.Entity;
    }

    public async Task<CarnivalBlockMembersEntity?> UpdateAsync(int id, CarnivalBlockMembersEntity entity, CancellationToken ct)
    {
        var existing = await DbSet.FindAsync([id], ct);
        if (existing == null) return null;
        existing.Role = entity.Role;
        existing.UpdatedAt = DateTime.UtcNow;
        DbSet.Update(existing);
        await AppDbContext.SaveChangesAsync(ct);
        return existing;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var entity = await DbSet.FindAsync([id], ct);
        if (entity == null) return false;
        DbSet.Remove(entity);
        return await AppDbContext.SaveChangesAsync(ct) > 0;
    }

    Task<IList<CarnivalBlockMembersEntity>> ICarnivalBlockMembersRepository.GetAllAsync(CancellationToken ct) => base.GetAllAsync();

    Task<CarnivalBlockMembersEntity?> ICarnivalBlockMembersRepository.GetByIdAsync(int id, CancellationToken ct) => base.GetByIdAsync(id);

    public async Task<IList<CarnivalBlockMembersEntity>> GetByBlockIdAsync(int blockId, CancellationToken ct)
    {
        return await DbSet
            .AsNoTracking()
            .Where(cbMember => cbMember.CarnivalBlockId == blockId)
            .ToListAsync(ct);
    }
}