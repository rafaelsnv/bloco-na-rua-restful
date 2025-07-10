using BlocoNaRua.Core.Models;

namespace BlocoNaRua.Domain.Entities;

public class CarnivalBlockEntity(int id) : EntityBase(id)
{
    public string Name { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public string InviteCode { get; set; } = string.Empty;
    public string ManagersInviteCode { get; set; } = string.Empty;
    public string CarnivalBlockImage { get; set; } = string.Empty;
    public ICollection<CarnivalBlockMembersEntity> CarnivalBlockMembers { get; set; } = [];
    public ICollection<MeetingEntity> Meetings { get; set; } = [];
    public ICollection<MeetingPresenceEntity> Presences { get; set; } = [];

    public CarnivalBlockEntity(
        int id,
        string name,
        string owner,
        string inviteCode,
        string managersInviteCode,
        string carnivalBlockImage
    ) : this(id)
    {
        Name = name;
        Owner = owner;
        InviteCode = inviteCode;
        ManagersInviteCode = managersInviteCode;
        CarnivalBlockImage = carnivalBlockImage;
    }
}
