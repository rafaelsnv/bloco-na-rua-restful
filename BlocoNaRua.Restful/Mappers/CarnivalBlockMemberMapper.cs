using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Restful.Models.CarnivalBlockMember;

namespace BlocoNaRua.Restful.Mappers;

public static class CarnivalBlockMemberMapper
{
    public static CarnivalBlockMemberResponse ToDTO(CarnivalBlockMembersEntity entity)
    {
        return new CarnivalBlockMemberResponse(
            entity.Id,
            entity.CarnivalBlockId,
            entity.MemberId,
            entity.Role,
            entity.CreatedAt.GetValueOrDefault(),
            entity.UpdatedAt.GetValueOrDefault()
        );
    }
}
