using BlocoNaRua.Domain.Entities;

namespace BlocoNaRua.Data.Repositories.Interfaces;

public interface IMembersRepository
{
    Task<IList<MemberEntity>> GetAllAsync(int? skip, int? take, CancellationToken ct);
    Task<MemberEntity?> GetByIdAsync(int id, CancellationToken ct);
    Task<MemberEntity> AddAsync(MemberEntity entity, CancellationToken ct);
    Task<bool> DeleteAsync(MemberEntity entity, CancellationToken ct);
    Task<bool> UpdateAsync(MemberEntity entity, CancellationToken ct);
    Task<MemberEntity?> GetByUuidAsync(Guid uuid, CancellationToken ct);
}