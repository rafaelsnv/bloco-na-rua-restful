namespace BlocoNaRua.Core.Models;

public abstract class EntityBase(int id)
{
    public int Id { get; private set; } = id;
}