using BlocoNaRua.Domain.Entities;

namespace BlocoNaRua.Services.Interfaces;

public interface ICarnivalBlockService
{
    Task<IList<CarnivalBlockEntity>> GetAllAsync();
    Task<CarnivalBlockEntity?> GetByIdAsync(int id);
    Task<CarnivalBlockEntity> CreateAsync(CarnivalBlockEntity entity);
    Task<CarnivalBlockEntity?> UpdateAsync(int id, int memberId, CarnivalBlockEntity entity);
    Task<bool> DeleteAsync(int id, int memberId);
}
