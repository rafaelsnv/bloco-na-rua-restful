namespace BlocoNaRua.Restful.Models.Member;

public record MemberUpdate(
    string Name,
    string Email,
    string Phone,
    string ProfileImage
);
