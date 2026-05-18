using BlocoNaRua.Domain.Entities;

namespace BlocoNaRua.Services.Interfaces;

public interface ICarnivalBlockService
{
    Task<IList<CarnivalBlockEntity>> GetAllAsync(int? page = null, int? pageSize = null);
    Task<CarnivalBlockEntity?> GetByIdAsync(int id);
    Task<CarnivalBlockEntity> CreateAsync(CarnivalBlockEntity entity);
    Task<CarnivalBlockEntity?> UpdateAsync(int id, int loggedMember, CarnivalBlockEntity entity);
    Task<bool> DeleteAsync(int id, int loggedMember);
}
