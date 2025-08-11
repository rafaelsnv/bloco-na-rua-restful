namespace BlocoNaRua.Restful.Models.Meeting;

public record MeetingCreate(
    string Name,
    string Description,
    string Location,
    DateTime? MeetingDateTime,
    int CarnivalBlockId
);
