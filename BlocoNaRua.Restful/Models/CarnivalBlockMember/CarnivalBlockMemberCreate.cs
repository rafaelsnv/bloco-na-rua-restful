using BlocoNaRua.Domain.Enums;

namespace BlocoNaRua.Restful.Models.CarnivalBlockMember;

public record class CarnivalBlockMemberCreate
(
    int CarnivalBlockId,
    RolesEnum Role
);
