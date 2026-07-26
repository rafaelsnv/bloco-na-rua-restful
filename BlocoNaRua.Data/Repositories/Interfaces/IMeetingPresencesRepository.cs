using BlocoNaRua.Domain.Entities;

namespace BlocoNaRua.Data.Repositories.Interfaces;

public interface IMeetingPresencesRepository
{
    Task<IList<MeetingPresenceEntity>> GetAllAsync(CancellationToken ct);
    Task<MeetingPresenceEntity?> GetByIdAsync(int id, CancellationToken ct);
    Task<IList<MeetingPresenceEntity>> GetByMeetingIdAsync(int meetingId, CancellationToken ct);
    Task<MeetingPresenceEntity> AddAsync(MeetingPresenceEntity entity, CancellationToken ct);
    Task<MeetingPresenceEntity?> UpdateAsync(int id, MeetingPresenceEntity entity, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
}