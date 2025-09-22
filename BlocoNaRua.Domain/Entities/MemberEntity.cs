using BlocoNaRua.Core.Models;

namespace BlocoNaRua.Domain.Entities;

public class MemberEntity(int id) : EntityBase(id)
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string ProfileImage { get; set; } = string.Empty;
    public Guid Uuid { get; set; } = new Guid();
    public ICollection<CarnivalBlockMembersEntity> CarnivalBlockMembers { get; set; } = [];
    public ICollection<MeetingPresenceEntity> Presences { get; set; } = [];

    public MemberEntity(
        int id,
        string name,
        string email,
        string phone,
        string profileImage,
        Guid uuid
    ) : this(id)
    {
        Name = name;
        Email = email;
        Phone = phone;
        Uuid = uuid;
        ProfileImage = profileImage;
    }
}
