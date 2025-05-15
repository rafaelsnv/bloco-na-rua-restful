using BlocoNaRua.Core.Models;

namespace BlocoNaRua.Domain.Entities;

public class UserEntity(int id) : EntityBase(id)
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string ProfileImage { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public List<CarnivalBlockUserEntity> CarnivalBlockUsers { get; set; } = [];
    public List<MeetingAttendanceEntity> MeetingAttendances { get; set; } = [];

}
