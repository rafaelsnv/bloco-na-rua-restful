using BlocoNaRua.Domain.Entities;
namespace BlocoNaRua.Services.Interfaces;

public interface IMembersService
{
    Task<IList<MemberEntity>> GetAllAsync();
    Task<MemberEntity?> GetByIdAsync(int id);
    Task<MemberEntity?> GetByUuidAsync(Guid uuid);
    Task<IList<CarnivalBlockMembersEntity>> GetMemberBlocksAsync(int memberId);
    Task<IList<MeetingEntity>> GetMemberMeetingsAsync(int memberId);
    Task<MemberEntity> CreateAsync(MemberEntity entity);
    Task<MemberEntity?> UpdateAsync(int id, int loggedMember, MemberEntity entity);
    Task<bool> DeleteAsync(int id, int loggedMember);
}
