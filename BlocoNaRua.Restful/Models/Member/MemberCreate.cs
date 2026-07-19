using System.ComponentModel.DataAnnotations;

namespace BlocoNaRua.Restful.Models.Member;

public record MemberCreate(
    string Name,
    [property: EmailAddress][property: RegularExpression(@"^[\w\.-]+@[\w\.-]+\.\w{2,}$", ErrorMessage = "Invalid email format")] string Email,
    [property: Phone][property: RegularExpression(@"^\+?[\d\s\-]{8,20}$", ErrorMessage = "Invalid phone format")] string Phone,
    string ProfileImage,
    string Uuid
);
