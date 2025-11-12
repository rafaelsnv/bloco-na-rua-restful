using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Restful.Models.Meeting;

namespace BlocoNaRua.Restful.Mappers;

public static class MeetingMapper
{
    public static MeetingDTO ToDTO(MeetingEntity entity)
    {
        return new MeetingDTO(
            entity.Id,
            entity.Name,
            entity.Description,
            entity.Location,
            entity.MeetingCode,
            entity.MeetingDateTime,
            entity.CarnivalBlockId,
            entity.CreatedAt,
            entity.UpdatedAt
        );
    }
}
