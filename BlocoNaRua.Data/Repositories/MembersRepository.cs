using BlocoNaRua.Data.Context;
using BlocoNaRua.Data.Repositories.Base;
using BlocoNaRua.Data.Repositories.Interfaces;
using BlocoNaRua.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlocoNaRua.Data.Repositories;

public class MembersRepository(AppDbContext appContext) : RepositoryBase<MemberEntity>(appContext), IMembersRepository
{
    public async Task<IList<MemberEntity>> GetAllAsync(int? skip, int? take, CancellationToken ct)
    {
        var query = DbSet.AsNoTracking();
        if (skip.HasValue) query = query.Skip(skip.Value);
        if (take.HasValue) query = query.Take(take.Value);
        return await query.ToListAsync(ct);
    }

    public async Task<MemberEntity?> GetByUuidAsync(Guid uuid, CancellationToken ct)
    {
        return await DbSet.AsNoTracking().FirstOrDefaultAsync(m => m.Uuid == uuid, ct);
    }

    public async Task<MemberEntity> AddAsync(MemberEntity entity, CancellationToken ct)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = entity.CreatedAt;
        var result = await DbSet.AddAsync(entity, ct);
        await AppDbContext.SaveChangesAsync(ct);
        return result.Entity;
    }

    public async Task<bool> DeleteAsync(MemberEntity entity, CancellationToken ct)
    {
        DbSet.Remove(entity);
        return await AppDbContext.SaveChangesAsync(ct) > 0;
    }

    public async Task<bool> UpdateAsync(MemberEntity entity, CancellationToken ct)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        DbSet.Update(entity);
        return await AppDbContext.SaveChangesAsync(ct) > 0;
    }

    Task<MemberEntity?> IMembersRepository.GetByIdAsync(int id, CancellationToken ct) => base.GetByIdAsync(id);
}