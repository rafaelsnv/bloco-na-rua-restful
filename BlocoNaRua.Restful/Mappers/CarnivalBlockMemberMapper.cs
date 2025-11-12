using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Restful.Models.CarnivalBlock;
using BlocoNaRua.Restful.Models.CarnivalBlockMember;

namespace BlocoNaRua.Restful.Mappers;

public static class CarnivalBlockMemberMapper
{
    public static CarnivalBlockMemberResponseDTO ToCarnivalBlockMemberResponseDTO(CarnivalBlockMembersEntity entity)
    {
        return new CarnivalBlockMemberResponseDTO
        {
            Id = entity.Id,
            MemberId = entity.MemberId,
            CarnivalBlockId = entity.CarnivalBlockId,
            Role = entity.Role,
            CreatedAt = entity.CreatedAt.GetValueOrDefault(),
            CarnivalBlock = new CarnivalBlockDTO(
                entity.CarnivalBlock.Id,
                entity.CarnivalBlock.OwnerId,
                entity.CarnivalBlock.Name,
                entity.CarnivalBlock.InviteCode,
                entity.CarnivalBlock.ManagersInviteCode,
                entity.CarnivalBlock.CarnivalBlockImage,
                entity.CarnivalBlock.CreatedAt.GetValueOrDefault(),
                entity.CarnivalBlock.UpdatedAt.GetValueOrDefault()
            )
        };
    }
}
