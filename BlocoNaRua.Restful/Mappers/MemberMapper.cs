using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Restful.Models.Member;

namespace BlocoNaRua.Restful.Mappers;

public static class MemberMapper
{
    public static MemberResponse ToDTO(MemberEntity entity)
    {
        return new MemberResponse(
            entity.Id,
            entity.Name,
            entity.Email,
            entity.Phone,
            entity.ProfileImage,
            entity.Uuid,
            entity.CreatedAt,
            entity.UpdatedAt
        );
    }
}
