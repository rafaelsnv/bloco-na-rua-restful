namespace BlocoNaRua.Restful.Models.Meeting;

public record MeetingUpdate(
    string Name,
    string Description,
    string Location,
    DateTime? MeetingDateTime
);
