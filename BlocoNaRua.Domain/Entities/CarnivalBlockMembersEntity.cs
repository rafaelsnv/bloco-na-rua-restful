using BlocoNaRua.Core.Models;
using BlocoNaRua.Domain.Enums;

namespace BlocoNaRua.Domain.Entities;

public class CarnivalBlockMembersEntity(int id) : EntityBase(id)
{
    public int CarnivalBlockId { get; set; }
    public int MemberId { get; set; }
    public RolesEnum Role { get; set; }
    public MemberEntity Member { get; set; } = null!;
    public CarnivalBlockEntity CarnivalBlock { get; set; } = null!;

    public CarnivalBlockMembersEntity(
        int id,
        int carnivalBlockId,
        int memberId,
        RolesEnum role
    ) : this(id)
    {
        CarnivalBlockId = carnivalBlockId;
        MemberId = memberId;
        Role = role;
    }
}
