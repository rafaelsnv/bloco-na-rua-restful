using BlocoNaRua.Domain.Entities;

namespace BlocoNaRua.Services.Interfaces;

public interface IMembersService
{
    Task<IList<MemberEntity>> GetAllAsync();
    Task<MemberEntity?> GetByIdAsync(int id);
    Task<MemberEntity> CreateAsync(MemberEntity entity);
    Task<MemberEntity?> UpdateAsync(int id, MemberEntity entity);
    Task<bool> DeleteAsync(int id);
}
