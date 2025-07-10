namespace BlocoNaRua.Restful.Models.CarnivalBlock;

public record class CarnivalBlock(
    int Id,
    string Name,
    string Owner,
    string InviteCode,
    string ManagersInviteCode,
    string CarnivalBlockImage,
    DateTime? CreatedAt,
    DateTime? UpdatedAt
);
