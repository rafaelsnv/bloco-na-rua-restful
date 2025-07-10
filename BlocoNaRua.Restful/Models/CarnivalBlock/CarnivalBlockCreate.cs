namespace BlocoNaRua.Restful.Models.CarnivalBlock;

public record class CarnivalBlockCreate(
    string Name,
    int OwnerId,
    string CarnivalBlockImage
);
