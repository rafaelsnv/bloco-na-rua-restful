namespace BlocoNaRua.Restful.Models.MeetingPresence;

public record MeetingPresenceCreate(
    int MemberId,
    int MeetingId,
    int CarnivalBlockId,
    bool IsPresent
);
