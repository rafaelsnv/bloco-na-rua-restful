using BlocoNaRua.Core.Models;

namespace BlocoNaRua.Domain.Entities;

public class MeetingEntity(int id) : EntityBase(id)
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string MeetingCode { get; set; } = string.Empty;
    public DateTime? MeetingDateTime { get; set; } = new DateTime(1900, 1, 1);
    public int CarnivalBlockId { get; set; }
    public CarnivalBlockEntity CarnivalBlock { get; set; } = null!;
    public ICollection<MeetingPresenceEntity> Presences { get; set; } = [];
    public MeetingEntity(
        int id,
        string name,
        string description,
        string location,
        string meetingCode,
        DateTime? meetingDateTime,
        int carnivalBlockId
    ) : this(id)
    {
        Name = name;
        Description = description;
        Location = location;
        MeetingCode = meetingCode;
        MeetingDateTime = meetingDateTime ?? new DateTime(1900, 1, 1);
        CarnivalBlockId = carnivalBlockId;
    }
}
