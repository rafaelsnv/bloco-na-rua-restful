using BlocoNaRua.Domain.Enums;

namespace BlocoNaRua.Restful.Models.CarnivalBlockMember;

public record CarnivalBlockMemberJoinResponse(
    int Id,
    int CarnivalBlockId,
    string CarnivalBlockName,
    int MemberId,
    RolesEnum Role,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
