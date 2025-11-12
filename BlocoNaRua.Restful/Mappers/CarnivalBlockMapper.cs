using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Restful.Models.CarnivalBlock;

namespace BlocoNaRua.Restful.Mappers;

public static class CarnivalBlockMapper
{
    public static CarnivalBlockResponse ToDTO(CarnivalBlockEntity entity)
    {
        return new CarnivalBlockResponse(
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
