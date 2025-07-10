using System.Linq.Expressions;

namespace BlocoNaRua.Core.Models;

public interface IRepositoryBase<TEntity> where TEntity : EntityBase
{
    Task<List<TEntity>> GetAllAsync();
    Task<TEntity?> GetByIdAsync(int id);
    Task<TEntity> AddAsync(TEntity entity);
    // Task<bool> AddRangeAsync(List<TEntity> entityList); //TODO
    Task DeleteAsync(TEntity entity);
    Task<int> UpdateAsync(TEntity entity);
    // Task<int> UpdateRangeAsync(List<TEntity> entityList); //TODO
}
