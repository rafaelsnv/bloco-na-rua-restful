namespace BlocoNaRua.Restful.Models.Member;

public record MemberCreate(
    string Name,
    string Email,
    string Phone,
    string ProfileImage,
    string Uuid
);
