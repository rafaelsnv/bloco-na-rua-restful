namespace BlocoNaRua.Restful.Models.Meeting;

public record MeetingResponse(
    int Id,
    string Name,
    string Description,
    string Location,
    string MeetingCode,
    DateTime? MeetingDateTime,
    int CarnivalBlockId,
    DateTime? CreatedAt,
    DateTime? UpdatedAt
);
