using BlocoNaRua.Domain.Enums;

namespace BlocoNaRua.Restful.Models.CarnivalBlockMember;

public record class CarnivalBlockMemberUpdate
(
    int CarnivalBlockId,
    RolesEnum Role
);
