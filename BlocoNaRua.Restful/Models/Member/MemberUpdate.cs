namespace BlocoNaRua.Restful.Models.Member;

public record MemberUpdate(
    int RequesterId,
    string Name,
    string Email,
    string Phone,
    string ProfileImage
);
