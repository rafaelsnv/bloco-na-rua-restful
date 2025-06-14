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
    public List<MeetingPresenceEntity> Presences { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
    public CarnivalBlockEntity CarnivalBlock { get; set; } = null!;

}
