using BlocoNaRua.Domain.Enums;

namespace BlocoNaRua.Restful.Models.CarnivalBlockMember;

public record class CarnivalBlockMemberUpdate
(
    int CarnivalBlockId,
    int LoggedMemberId,
    RolesEnum Role
);
