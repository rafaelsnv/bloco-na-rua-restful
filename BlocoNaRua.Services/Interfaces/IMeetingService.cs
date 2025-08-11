using BlocoNaRua.Domain.Entities;

namespace BlocoNaRua.Services.Interfaces;

public interface IMeetingService
{
    Task<IList<MeetingEntity>> GetAllAsync();
    Task<IList<MeetingEntity>> GetAllByBlockIdAsync(int blockId);
    Task<MeetingEntity?> GetByIdAsync(int id);
    Task<MeetingEntity> CreateAsync(MeetingEntity model, int loggedMember);
    Task<MeetingEntity?> UpdateAsync(int id, MeetingEntity model, int loggedMember);
    Task<bool> DeleteAsync(int id, int loggedMember);
}
