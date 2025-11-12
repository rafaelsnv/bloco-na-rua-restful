using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Restful.Models.MeetingPresence;

namespace BlocoNaRua.Restful.Mappers;

public static class MeetingPresenceMapper
{
    public static MeetingPresenceDTO ToDTO(MeetingPresenceEntity entity)
    {
        return new MeetingPresenceDTO(
            entity.Id,
            entity.MemberId,
            entity.MeetingId,
            entity.CarnivalBlockId,
            entity.IsPresent,
            entity.CreatedAt,
            entity.UpdatedAt
        );
    }
}
