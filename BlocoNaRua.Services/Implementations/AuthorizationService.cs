using BlocoNaRua.Data.Repositories.Interfaces;
using BlocoNaRua.Domain.Enums;
using BlocoNaRua.Services.Interfaces;

namespace BlocoNaRua.Services.Implementations;

public class AuthorizationService
(
    ICarnivalBlocksRepository carnivalBlocksRepository,
    ICarnivalBlockMembersRepository carnivalBlockMembersRepository
) : IAuthorizationService
{
    private readonly ICarnivalBlocksRepository _carnivalBlocksRepository = carnivalBlocksRepository;
    private readonly ICarnivalBlockMembersRepository _carnivalBlockMembersRepository = carnivalBlockMembersRepository;

    public async Task<RolesEnum?> GetMemberRole(int carnivalBlockId, int memberId)
    {
        var carnivalBlock = await _carnivalBlocksRepository.GetByIdAsync(carnivalBlockId)
            ?? throw new KeyNotFoundException("Carnival block does not exist.");

        if (carnivalBlock.OwnerId == memberId)
        {
            return RolesEnum.Owner;
        }

        return await _carnivalBlockMembersRepository.GetMemberRole(carnivalBlockId, memberId);
    }
}
