using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Domain.Enums;

namespace BlocoNaRua.Services.Interfaces;

public interface ICarnivalBlockMembersService
{
    Task<List<CarnivalBlockMembersEntity>> GetAllAsync();
    Task<IList<CarnivalBlockMembersEntity>> GetByBlockIdAsync(int blockId);
    Task<IList<CarnivalBlockMembersEntity>> GetByMemberIdAsync(int memberId);
    Task CreateAsync(CarnivalBlockMembersEntity carnivalBlockMember, int loggedMemberId);
    Task<CarnivalBlockMembersEntity?> UpdateAsync(int id, int loggedMemberId, RolesEnum newRole);
    Task<bool> DeleteAsync(int id, int loggedMemberId);
}
