namespace BlocoNaRua.Restful.Models.MeetingPresence;

public record MeetingPresenceResponse(
    int Id,
    int MemberId,
    int MeetingId,
    int CarnivalBlockId,
    bool IsPresent,
    DateTime? CreatedAt,
    DateTime? UpdatedAt
);
