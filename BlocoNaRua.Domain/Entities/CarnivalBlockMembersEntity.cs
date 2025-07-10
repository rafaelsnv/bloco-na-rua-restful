using BlocoNaRua.Core.Models;

namespace BlocoNaRua.Domain.Entities;

public class CarnivalBlockMembersEntity(int id) : EntityBase(id)
// Entidade que representa a relação entre um bloco de carnaval e um usuário
{
    public int CarnivalBlockId { get; set; }
    public int MemberId { get; set; }
    public string Role { get; set; } = string.Empty; // "Manager", "Percussionist", etc.
    public MemberEntity Member { get; set; } = null!;
    public CarnivalBlockEntity CarnivalBlock { get; set; } = null!;
}
