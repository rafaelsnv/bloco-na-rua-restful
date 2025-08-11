using BlocoNaRua.Domain.Enums;

namespace BlocoNaRua.Services.Interfaces;

public interface IAuthorizationService
{
    Task<RolesEnum?> GetMemberRole(int carnivalBlockId, int memberId);
}
