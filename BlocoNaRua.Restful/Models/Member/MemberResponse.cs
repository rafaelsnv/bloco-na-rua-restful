namespace BlocoNaRua.Restful.Models.Member;

public record MemberResponse(
    int Id,
    string Name,
    string Email,
    string Phone,
    string ProfileImage,
    Guid Uuid,
    DateTime? CreatedAt,
    DateTime? UpdatedAt
);
