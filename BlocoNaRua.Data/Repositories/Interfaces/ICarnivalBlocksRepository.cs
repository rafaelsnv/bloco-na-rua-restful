using BlocoNaRua.Domain.Entities;

namespace BlocoNaRua.Data.Repositories.Interfaces;

public interface ICarnivalBlocksRepository
{
    Task<IList<CarnivalBlockEntity>> GetAllAsync(CancellationToken ct);
    Task<CarnivalBlockEntity?> GetByIdAsync(int id, CancellationToken ct);
    Task<CarnivalBlockEntity?> GetByInviteCodeAsync(string inviteCode, CancellationToken ct);
    Task<CarnivalBlockEntity> AddAsync(CarnivalBlockEntity entity, CancellationToken ct);
    Task<CarnivalBlockEntity?> UpdateAsync(int id, CarnivalBlockEntity entity, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
}