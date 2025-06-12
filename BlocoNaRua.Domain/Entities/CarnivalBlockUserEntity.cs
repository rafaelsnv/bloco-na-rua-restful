using BlocoNaRua.Core.Models;

namespace BlocoNaRua.Domain.Entities;



public class CarnivalBlockUsersEntity(int id) : EntityBase(id)
// Entidade que representa a relação entre um bloco de carnaval e um usuário
{
    public int CarnivalBlockId { get; set; }
    public int UserId { get; set; }
    public string Role { get; set; } = string.Empty; // "Manager", "Percussionist", etc.
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
    public UserEntity User { get; set; } = null!;
    public CarnivalBlockEntity CarnivalBlock { get; set; } = null!;
}
