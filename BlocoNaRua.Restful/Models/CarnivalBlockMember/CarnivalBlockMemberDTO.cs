using BlocoNaRua.Domain.Enums;

namespace BlocoNaRua.Restful.Models.CarnivalBlockMember;

public record CarnivalBlockMemberDTO
(
    int Id,
    int CarnivalBlockId,
    int MemberId,
    RolesEnum Role,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
