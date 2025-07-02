using BlocoNaRua.Core.Models;

namespace BlocoNaRua.Domain.Entities;

public class MemberEntity(int id) : EntityBase(id)
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string ProfileImage { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<CarnivalBlockMembersEntity> CarnivalBlockMembers { get; set; } = [];
    public ICollection<MeetingPresenceEntity> Presences { get; set; } = [];

    public MemberEntity(
        int id,
        string name,
        string email,
        string password,
        string phone,
        string profileImage,
        DateTime? createdAt = null,
        DateTime? updatedAt = null
    ) : this(id)
    {
        Name = name;
        Email = email;
        Password = password;
        Phone = phone;
        ProfileImage = profileImage;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }
}
