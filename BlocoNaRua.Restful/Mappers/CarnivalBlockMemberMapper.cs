using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Restful.Models.CarnivalBlockMember;

namespace BlocoNaRua.Restful.Mappers;

public static class CarnivalBlockMemberMapper
{
    public static CarnivalBlockMemberDTO ToDTO(CarnivalBlockMembersEntity entity)
    {
        return new CarnivalBlockMemberDTO(
            entity.Id,
            entity.CarnivalBlockId,
            entity.MemberId,
            entity.Role,
            entity.CreatedAt.GetValueOrDefault(),
            entity.UpdatedAt.GetValueOrDefault()
        );
    }
}
