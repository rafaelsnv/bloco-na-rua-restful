using BlocoNaRua.Core.Models;

namespace BlocoNaRua.Domain.Entities;



public class CarnivalBlockUserEntity(int id) : EntityBase(id) // Entidade que representa a relação entre um bloco de carnaval e um usuário
{
    public int CarnivalBlockId { get; set; }
    public CarnivalBlockEntity CarnivalBlock { get; set; } = null!;
    public int UserId { get; set; }
    public UserEntity User { get; set; } = null!;
    public string Role { get; set; } = string.Empty; // "Manager", "Percussionist", etc.
}
