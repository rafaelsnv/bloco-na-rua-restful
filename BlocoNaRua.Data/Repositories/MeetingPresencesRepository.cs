using BlocoNaRua.Data.Context;
using BlocoNaRua.Data.Repositories.Base;
using BlocoNaRua.Data.Repositories.Interfaces;
using BlocoNaRua.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlocoNaRua.Data.Repositories;

public class MeetingPresencesRepository(AppDbContext appContext) : RepositoryBase<MeetingPresenceEntity>(appContext), IMeetingPresencesRepository
{
    public async Task<IList<MeetingPresenceEntity>> GetAllAsync(CancellationToken ct)
    {
        return await DbSet.AsNoTracking().ToListAsync(ct);
    }

    public async Task<IList<MeetingPresenceEntity>> GetByMeetingIdAsync(int meetingId, CancellationToken ct)
    {
        return await DbSet.AsNoTracking().Where(p => p.MeetingId == meetingId).ToListAsync(ct);
    }

    public async Task<MeetingPresenceEntity> AddAsync(MeetingPresenceEntity entity, CancellationToken ct)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = entity.CreatedAt;
        var result = await DbSet.AddAsync(entity, ct);
        await AppDbContext.SaveChangesAsync(ct);
        return result.Entity;
    }

    public async Task<MeetingPresenceEntity?> UpdateAsync(int id, MeetingPresenceEntity entity, CancellationToken ct)
    {
        var existing = await DbSet.FindAsync([id], ct);
        if (existing == null) return null;
        existing.IsPresent = entity.IsPresent;
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

    Task<MeetingPresenceEntity?> IMeetingPresencesRepository.GetByIdAsync(int id, CancellationToken ct) => base.GetByIdAsync(id);
}