namespace BlocoNaRua.Restful.Models.Member;

public record class MemberCreate(
    string Name,
    string Email,
    string Password,
    string Phone,
    string ProfileImage
);
