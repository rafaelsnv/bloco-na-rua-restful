using BlocoNaRua.Domain.Entities;

namespace BlocoNaRua.Services.Interfaces;

public interface IMeetingPresenceService
{
    Task<IList<MeetingPresenceEntity>> GetAllAsync();
    Task<MeetingPresenceEntity?> GetByIdAsync(int id);
    Task<MeetingPresenceEntity> CreateAsync(MeetingPresenceEntity model, int loggedMember);
    Task<MeetingPresenceEntity?> UpdateAsync(int id, MeetingPresenceEntity model, int loggedMember);
    Task<bool> DeleteAsync(int id, int loggedMember);
}
