namespace BlocoNaRua.Restful.Models.CarnivalBlock;

public record CarnivalBlockResponse(
    int Id,
    int OwnerId,
    string Name,
    string InviteCode,
    string ManagersInviteCode,
    string CarnivalBlockImage,
    DateTime? CreatedAt,
    DateTime? UpdatedAt
);
