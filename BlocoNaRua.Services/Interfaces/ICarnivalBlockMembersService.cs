using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Domain.Enums;

namespace BlocoNaRua.Services.Interfaces;

public interface ICarnivalBlockMembersService
{
    Task<List<CarnivalBlockMembersEntity>> GetAllAsync();
    Task<CarnivalBlockMembersEntity?> GetByIdAsync(int id);
    Task CreateAsync(CarnivalBlockMembersEntity carnivalBlockMember);
    Task<CarnivalBlockMembersEntity?> UpdateAsync(int id, int loggedMemberId, RolesEnum newRole);
    Task<bool> DeleteAsync(int id, int loggedMemberId);
}
