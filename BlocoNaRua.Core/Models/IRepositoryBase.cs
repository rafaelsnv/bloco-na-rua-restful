namespace BlocoNaRua.Core.Models;

public interface IRepositoryBase<TEntity> where TEntity : EntityBase
{
    Task<IList<TEntity>> GetAllAsync();
    Task<TEntity> GetByIdAsync(int id);
    Task<TEntity> AddAsync(TEntity entity);
    Task<bool> DeleteAsync(TEntity entity);
    Task<bool> UpdateAsync(TEntity entity);
}
