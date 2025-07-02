using BlocoNaRua.Core.Models;

namespace BlocoNaRua.Domain.Entities;

public class MeetingPresenceEntity(int id) : EntityBase(id) // Entidade que representa a presença de um usuário em um encontro
{
    public int MemberId { get; set; }
    public int MeetingId { get; set; }
    public int CarnivalBlockId { get; set; }
    public bool IsPresent { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
    public MemberEntity Member { get; set; } = null!;
    public MeetingEntity Meeting { get; set; } = null!;
    public CarnivalBlockEntity CarnivalBlock { get; set; } = null!;
}
