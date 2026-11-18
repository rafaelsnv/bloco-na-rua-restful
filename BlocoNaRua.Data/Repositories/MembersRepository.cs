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

    public async Task<MemberEntity?> GetByUuidIgnoreFiltersAsync(Guid uuid, CancellationToken ct)
    {
        // IgnoreQueryFilters: a soft deleted member must still be found so signup cleanup can hard delete it.
        return await DbSet.IgnoreQueryFilters().FirstOrDefaultAsync(m => m.Uuid == uuid, ct);
    }

    public async Task<MemberEntity?> GetByProviderAsync(string provider, string providerId, CancellationToken ct)
    {
        // IgnoreQueryFilters: a soft deleted member must still be found by its provider identity,
        // otherwise the insert below would break the unique ux_members_provider index.
        return await DbSet.IgnoreQueryFilters().FirstOrDefaultAsync(m => m.Provider == provider && m.ProviderId == providerId, ct);
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
        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;
        await AppDbContext.SaveChangesAsync(ct);
        AppDbContext.ChangeTracker.Clear();
        return true;
    }

    public async Task<MemberEntity?> GetByIdAsyncOverride(int id, CancellationToken ct)
    {
        return await DbSet.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id, ct);
    }

    Task<MemberEntity?> IMembersRepository.GetByIdAsync(int id, CancellationToken ct) => GetByIdAsyncOverride(id, ct);

    public async Task<bool> UpdateAsync(MemberEntity entity, CancellationToken ct)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        DbSet.Update(entity);
        return await AppDbContext.SaveChangesAsync(ct) > 0;
    }

    public async Task<bool> HardDeleteAsync(MemberEntity entity, CancellationToken ct)
    {
        DbSet.Remove(entity);
        await AppDbContext.SaveChangesAsync(ct);
        AppDbContext.ChangeTracker.Clear();
        return true;
    }

}