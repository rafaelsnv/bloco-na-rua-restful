using BlocoNaRua.Domain.Enums;
using BlocoNaRua.Restful.Models.CarnivalBlock;

namespace BlocoNaRua.Restful.Models.CarnivalBlockMember;

public class CarnivalBlockMemberResponseDTO
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public int CarnivalBlockId { get; set; }
    public RolesEnum Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public CarnivalBlockDTO? CarnivalBlock { get; set; }
}
