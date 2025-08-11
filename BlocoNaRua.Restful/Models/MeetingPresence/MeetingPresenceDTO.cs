namespace BlocoNaRua.Restful.Models.MeetingPresence;

public record MeetingPresenceDTO(
    int Id,
    int MemberId,
    int MeetingId,
    int CarnivalBlockId,
    bool IsPresent,
    DateTime? CreatedAt,
    DateTime? UpdatedAt
);
