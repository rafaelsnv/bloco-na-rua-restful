namespace BlocoNaRua.Core.Models;

public abstract class EntityBase(int id)
{
    public int Id { get; private set; } = id;
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
}
