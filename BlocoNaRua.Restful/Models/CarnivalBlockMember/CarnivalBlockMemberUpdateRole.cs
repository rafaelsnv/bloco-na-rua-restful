using BlocoNaRua.Domain.Enums;

namespace BlocoNaRua.Restful.Models.CarnivalBlockMember;

public record class CarnivalBlockMemberUpdateRole
(
    int CarnivalBlockId,
    int LoggedMemberId,
    RolesEnum Role
);
