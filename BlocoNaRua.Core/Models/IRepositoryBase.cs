using System.Linq.Expressions;

namespace BlocoNaRua.Core.Models;

public interface IRepositoryBase<TEntity> where TEntity : EntityBase
{
    Task<IEnumerable<TEntity>> GetAsync(Expression<Func<TEntity, bool>>? filter = null);
    Task<List<TEntity>> GetAllAsync();
    Task<TEntity> GetByIdAsync(List<int> id);
    Task<bool> AddAsync(TEntity entity);
    Task<bool> AddRangeAsync(List<TEntity> entityList);
    Task DeleteAsync(TEntity entity);
    Task UpdateAsync(TEntity entity);
    Task<int> UpdateRangeAsync(List<TEntity> entityList);
}