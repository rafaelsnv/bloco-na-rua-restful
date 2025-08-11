using BlocoNaRua.Domain.Enums;

namespace BlocoNaRua.Restful.Models.CarnivalBlockMember;

public record class CarnivalBlockMemberCreate
(
    int CarnivalBlockId,
    int MemberId,
    RolesEnum Role
);
