using BlocoNaRua.Core.Models;
using BlocoNaRua.Domain.Entities;

namespace BlocoNaRua.Data.Repositories.Interfaces;

public interface IMeetingsRepository : IRepositoryBase<MeetingEntity>
{
    Task<IList<MeetingEntity>> GetAllByBlockIdAsync(int blockId);
    Task<IList<MeetingEntity>> GetByBlockIdsAsync(List<int> blockIds);
}
