using BlocoNaRua.Data.Repositories.Interfaces;
using BlocoNaRua.Services.Interfaces;

namespace BlocoNaRua.Services.Implementations;

public class AdminService(IMembersRepository membersRepository) : IAdminService
{
    private readonly IMembersRepository _membersRepository = membersRepository;

    public async Task<(bool Deleted, string? ErrorMessage)> DeleteSignupAsync(Guid uuid)
    {
        try
        {
            // IgnoreQueryFilters: soft deleted members are hidden by the IsActive filter,
            // but signup cleanup must still find them to hard delete the row.
            var member = await _membersRepository.GetByUuidIgnoreFiltersAsync(uuid, CancellationToken.None);
            if (member is null)
                return (false, null);

            await _membersRepository.HardDeleteAsync(member, CancellationToken.None);
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}
