using BlocoNaRua.Core.Models;
using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Domain.Enums;

namespace BlocoNaRua.Data.Repositories.Interfaces;

public interface ICarnivalBlockMembersRepository : IRepositoryBase<CarnivalBlockMembersEntity>
{
    Task<RolesEnum?> GetMemberRole(int carnivalBlockId, int memberId);
    Task<IList<CarnivalBlockMembersEntity>> GetByBlockIdAsync(int blockId);
    Task<IList<CarnivalBlockMembersEntity>> GetByMemberIdAsync(int memberId);
}
