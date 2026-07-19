using BlocoNaRua.Domain.Entities;

namespace BlocoNaRua.Services.Interfaces;

public interface IMemberIdentityService
{
    Task<int> GetMemberIdAsync();
    Task<MemberEntity?> GetMemberAsync();
}