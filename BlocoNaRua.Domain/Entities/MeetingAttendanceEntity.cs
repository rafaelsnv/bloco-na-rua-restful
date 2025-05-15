using BlocoNaRua.Core.Models;

namespace BlocoNaRua.Domain.Entities;

public class MeetingAttendanceEntity(int id) : EntityBase(id) // Entidade que representa a presença de um usuário em um encontro
{
    public int MeetingId { get; set; }
    public MeetingEntity Meeting { get; set; } = null!;
    public int UserId { get; set; }
    public UserEntity User { get; set; } = null!;
    public bool IsPresent { get; set; } = true;
}
