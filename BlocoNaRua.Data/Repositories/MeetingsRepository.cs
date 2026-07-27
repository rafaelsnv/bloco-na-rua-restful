using BlocoNaRua.Data.Context;
using BlocoNaRua.Data.Repositories.Base;
using BlocoNaRua.Data.Repositories.Interfaces;
using BlocoNaRua.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlocoNaRua.Data.Repositories;

public class MeetingsRepository(AppDbContext appContext) : RepositoryBase<MeetingEntity>(appContext), IMeetingsRepository
{
    public async Task<IList<MeetingEntity>> GetAllAsync(int? skip, int? take, CancellationToken ct)
    {
        var query = DbSet.AsNoTracking();
        if (skip.HasValue) query = query.Skip(skip.Value);
        if (take.HasValue) query = query.Take(take.Value);
        return await query.ToListAsync(ct);
    }

    public async Task<IList<MeetingEntity>> GetByBlockIdsAsync(IList<int> blockIds, CancellationToken ct)
    {
        return await DbSet.Where(m => blockIds.Contains(m.CarnivalBlockId)).ToListAsync(ct);
    }

    public async Task<MeetingEntity> AddAsync(MeetingEntity entity, CancellationToken ct)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = entity.CreatedAt;
        if (entity.MeetingDateTime.HasValue && entity.MeetingDateTime.Value.Kind == DateTimeKind.Unspecified)
            entity.MeetingDateTime = DateTime.SpecifyKind(entity.MeetingDateTime.Value, DateTimeKind.Utc);
        var result = await DbSet.AddAsync(entity, ct);
        await AppDbContext.SaveChangesAsync(ct);
        return result.Entity;
    }

    public async Task<bool> UpdateAsync(MeetingEntity entity, CancellationToken ct)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        if (entity.MeetingDateTime.HasValue && entity.MeetingDateTime.Value.Kind == DateTimeKind.Unspecified)
            entity.MeetingDateTime = DateTime.SpecifyKind(entity.MeetingDateTime.Value, DateTimeKind.Utc);
        DbSet.Update(entity);
        return await AppDbContext.SaveChangesAsync(ct) > 0;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var entity = await DbSet.FindAsync([id], ct);
        if (entity == null) return false;
        DbSet.Remove(entity);
        return await AppDbContext.SaveChangesAsync(ct) > 0;
    }

    Task<MeetingEntity?> IMeetingsRepository.GetByIdAsync(int id, CancellationToken ct) => base.GetByIdAsync(id);
}