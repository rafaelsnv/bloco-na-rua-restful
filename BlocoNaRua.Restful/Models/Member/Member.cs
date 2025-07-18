namespace BlocoNaRua.Restful.Models.Member;

public record class Member(
    int Id,
    string Name,
    string Email,
    string Password,
    string Phone,
    string ProfileImage,
    DateTime? CreatedAt,
    DateTime? UpdatedAt
);
