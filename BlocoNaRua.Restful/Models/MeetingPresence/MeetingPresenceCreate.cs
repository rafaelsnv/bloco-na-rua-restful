namespace BlocoNaRua.Restful.Models.MeetingPresence;

public record MeetingPresenceCreate(
    int MeetingId,
    int CarnivalBlockId,
    bool IsPresent
);
