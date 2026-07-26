using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Domain.Enums;

namespace BlocoNaRua.Data.Repositories.Interfaces;

public interface ICarnivalBlockMembersRepository
{
    Task<IList<CarnivalBlockMembersEntity>> GetAllAsync(CancellationToken ct);
    Task<CarnivalBlockMembersEntity?> GetByIdAsync(int id, CancellationToken ct);
    Task<IList<CarnivalBlockMembersEntity>> GetByBlockIdAsync(int blockId, CancellationToken ct);
    Task<IList<CarnivalBlockMembersEntity>> GetByMemberIdAsync(int memberId, CancellationToken ct);
    Task<CarnivalBlockMembersEntity?> GetByMemberAndBlockAsync(int carnivalBlockId, int memberId, CancellationToken ct);
    Task<RolesEnum?> GetMemberRole(int carnivalBlockId, int memberId, CancellationToken ct);
    Task<CarnivalBlockMembersEntity> AddAsync(CarnivalBlockMembersEntity entity, CancellationToken ct);
    Task<CarnivalBlockMembersEntity?> UpdateAsync(int id, CarnivalBlockMembersEntity entity, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
}