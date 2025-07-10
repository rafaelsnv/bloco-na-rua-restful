using System.Linq.Expressions;
using BlocoNaRua.Core.Models;
using BlocoNaRua.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace BlocoNaRua.Data.Repositories.Base;

public class RepositoryBase<TEntity>(AppDbContext appContext) : IRepositoryBase<TEntity> where TEntity : EntityBase
{
    protected readonly DbSet<TEntity> DbSet = appContext.Set<TEntity>();
    protected readonly AppDbContext AppDbContext = appContext;

    public async Task<List<TEntity>> GetAllAsync()
    {
        return await DbSet.AsNoTracking().ToListAsync();
    }

    public async Task<TEntity?> GetByIdAsync(int id)
    {
        return await DbSet.FindAsync(id);
    }

    public async Task<TEntity> AddAsync(TEntity entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = entity.CreatedAt;
        var result = await DbSet.AddAsync(entity);
        await AppDbContext.SaveChangesAsync();
        return result.Entity;
    }

    public async Task DeleteAsync(TEntity entity)
    {
        DbSet.Remove(entity);
        await AppDbContext.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(TEntity entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        DbSet.Update(entity);
        return await AppDbContext.SaveChangesAsync() > 0;
    }
}
