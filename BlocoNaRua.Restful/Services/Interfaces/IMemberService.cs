using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Restful.Models.Member;

namespace BlocoNaRua.Restful.Services.Interfaces;

public interface IMemberService
{
    Task<IEnumerable<MemberEntity>> GetAllMembersAsync();
    Task<MemberEntity?> GetMemberByIdAsync(int id);
    Task<MemberEntity> CreateMemberAsync(MemberCreate createModel);
    Task<bool> UpdateMemberAsync(int id, Member updateModel);
    Task<bool> DeleteMemberAsync(int id);
} 
