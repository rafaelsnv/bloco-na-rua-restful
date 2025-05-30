using System.Linq.Expressions;

namespace BlocoNaRua.Core.Models;

public interface IRepositoryBase<TEntity> where TEntity : EntityBase
{
    Task<IEnumerable<TEntity>> GetAsync(Expression<Func<TEntity, bool>>? filter = null);
    // Task<List<TEntity>> GetAllAsync(); //TODO
    Task<TEntity> GetByIdAsync(int id);
    Task<int> AddAsync(TEntity entity);
    // Task<bool> AddRangeAsync(List<TEntity> entityList); //TODO
    Task DeleteAsync(TEntity entity);
    Task<int> UpdateAsync(TEntity entity);
    // Task<int> UpdateRangeAsync(List<TEntity> entityList); //TODO
}
