using BlocoNaRua.Domain.Entities;

namespace BlocoNaRua.Data.Repositories.Interfaces;

public interface IMeetingsRepository
{
    Task<IList<MeetingEntity>> GetAllAsync(int? skip, int? take, CancellationToken ct);
    Task<MeetingEntity?> GetByIdAsync(int id, CancellationToken ct);
    Task<IList<MeetingEntity>> GetByBlockIdsAsync(IList<int> blockIds, CancellationToken ct);
    Task<MeetingEntity> AddAsync(MeetingEntity entity, CancellationToken ct);
    Task<bool> UpdateAsync(MeetingEntity entity, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
}