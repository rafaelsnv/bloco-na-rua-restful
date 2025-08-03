namespace BlocoNaRua.Restful.Models.Member;

public record MemberDTO(
    int Id,
    string Name,
    string Email,
    string Phone,
    string ProfileImage
);
