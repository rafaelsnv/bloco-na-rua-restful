namespace BlocoNaRua.Restful.Models.CarnivalBlock;

public record class CarnivalBlockDTO(
    int Id,
    string Name,
    string OwnerId,
    string InviteCode,
    string ManagersInviteCode,
    string CarnivalBlockImage,
    DateTime? CreatedAt,
    DateTime? UpdatedAt
);
