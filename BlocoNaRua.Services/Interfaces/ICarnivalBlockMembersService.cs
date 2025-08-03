using BlocoNaRua.Domain.Entities;

namespace BlocoNaRua.Services.Interfaces;

public interface ICarnivalBlockMembersService
{
    Task<List<CarnivalBlockMembersEntity>> GetAllAsync();
    Task<CarnivalBlockMembersEntity?> GetByIdAsync(int id);
    Task CreateAsync(CarnivalBlockMembersEntity carnivalBlockMember);
}
