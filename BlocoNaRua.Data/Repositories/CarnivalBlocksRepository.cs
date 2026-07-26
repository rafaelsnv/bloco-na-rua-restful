using BlocoNaRua.Data.Context;
using BlocoNaRua.Data.Repositories.Base;
using BlocoNaRua.Data.Repositories.Interfaces;
using BlocoNaRua.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlocoNaRua.Data.Repositories;

public class CarnivalBlocksRepository(AppDbContext appContext) : RepositoryBase<CarnivalBlockEntity>(appContext), ICarnivalBlocksRepository
{
    public async Task<IList<CarnivalBlockEntity>> GetAllAsync(CancellationToken ct)
    {
        return await DbSet.AsNoTracking().ToListAsync(ct);
    }

    public async Task<CarnivalBlockEntity?> GetByInviteCodeAsync(string inviteCode, CancellationToken ct)
    {
        return await DbSet.AsNoTracking().FirstOrDefaultAsync(b => b.InviteCode == inviteCode, ct);
    }

    public async Task<CarnivalBlockEntity> AddAsync(CarnivalBlockEntity entity, CancellationToken ct)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = entity.CreatedAt;
        var result = await DbSet.AddAsync(entity, ct);
        await AppDbContext.SaveChangesAsync(ct);
        return result.Entity;
    }

    public async Task<CarnivalBlockEntity?> UpdateAsync(int id, CarnivalBlockEntity entity, CancellationToken ct)
    {
        var existing = await DbSet.FindAsync([id], ct);
        if (existing == null) return null;
        existing.Name = entity.Name;
        existing.CarnivalBlockImage = entity.CarnivalBlockImage;
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

    Task<CarnivalBlockEntity?> ICarnivalBlocksRepository.GetByIdAsync(int id, CancellationToken ct) => base.GetByIdAsync(id);
}