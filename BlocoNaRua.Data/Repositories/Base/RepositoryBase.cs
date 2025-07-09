using System.Linq.Expressions;
using BlocoNaRua.Core.Models;
using BlocoNaRua.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace BlocoNaRua.Data.Repositories.Base;

public class RepositoryBase<TEntity>(AppDbContext appContext) : IRepositoryBase<TEntity> where TEntity : EntityBase
{
    public readonly DbSet<TEntity> _DbSet = appContext.Set<TEntity>();
    public readonly AppDbContext _AppDbContext = appContext;

    public async Task<List<TEntity>> GetAllAsync()
    {
        return await _DbSet.AsNoTracking().ToListAsync();
    }

    public async Task<IEnumerable<TEntity>> GetAsync(Expression<Func<TEntity, bool>> filter = null)
    {
        var query = _DbSet.AsQueryable();

        if (filter != null)
            query = query
                .Where(filter)
                .AsNoTracking();

        return await query.ToListAsync();
    }

    public async Task<TEntity> GetByIdAsync(int id)
    {
        return await _DbSet.FindAsync(id);
    }

    public async Task<TEntity> AddAsync(TEntity entity)
    {
        var result = await _DbSet.AddAsync(entity);
        await _AppDbContext.SaveChangesAsync();
        return result.Entity;
    }

    public async Task DeleteAsync(TEntity entity)
    {
        _DbSet.Remove(entity);
        await _AppDbContext.SaveChangesAsync();
    }

    public async Task<int> UpdateAsync(TEntity entity)
    {
        _DbSet.Update(entity);
        return await _AppDbContext.SaveChangesAsync();
    }
}
