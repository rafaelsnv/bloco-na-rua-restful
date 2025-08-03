using BlocoNaRua.Core.Models;
using BlocoNaRua.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace BlocoNaRua.Data.Repositories.Base;

public class RepositoryBase<TEntity>(AppDbContext appContext) : IRepositoryBase<TEntity> where TEntity : EntityBase
{
    protected readonly DbSet<TEntity> DbSet = appContext.Set<TEntity>();
    protected readonly AppDbContext AppDbContext = appContext;

    public async Task<IList<TEntity>> GetAllAsync()
    {
        return await DbSet.AsNoTracking().ToListAsync();
    }

    public async Task<TEntity?> GetByIdAsync(int id)
    {
        var entity = await DbSet.FindAsync(id);
        return entity;
    }

    public async Task<TEntity> AddAsync(TEntity entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = entity.CreatedAt;
        var result = await DbSet.AddAsync(entity);
        await AppDbContext.SaveChangesAsync();
        return result.Entity;
    }

    public async Task<bool> DeleteAsync(TEntity entity)
    {
        DbSet.Remove(entity);
        return await AppDbContext.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateAsync(TEntity entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        DbSet.Update(entity);
        return await AppDbContext.SaveChangesAsync() > 0;
    }
}
