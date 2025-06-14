using BlocoNaRua.Core.Models;

namespace BlocoNaRua.Domain.Entities;

public class CarnivalBlockEntity(int id) : EntityBase(id)
{
    public string Name { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public string InviteCode { get; set; } = string.Empty;
    public string ManagersInviteCode { get; set; } = string.Empty;
    public string CarnivalBlockImage { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<CarnivalBlockUsersEntity> CarnivalBlockUsers { get; set; } = [];
    public ICollection<MeetingEntity> Meetings { get; set; } = [];
    public ICollection<MeetingPresenceEntity> Presences { get; set; } = [];
}
