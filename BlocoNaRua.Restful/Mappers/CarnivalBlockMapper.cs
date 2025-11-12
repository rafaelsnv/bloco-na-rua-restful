using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Restful.Models.CarnivalBlock;

namespace BlocoNaRua.Restful.Mappers;

public static class CarnivalBlockMapper
{
    public static CarnivalBlockDTO ToDTO(CarnivalBlockEntity entity)
    {
        return new CarnivalBlockDTO(
            entity.Id,
            entity.OwnerId,
            entity.Name,
            entity.InviteCode,
            entity.ManagersInviteCode,
            entity.CarnivalBlockImage,
            entity.CreatedAt,
            entity.UpdatedAt
        );
    }
}
