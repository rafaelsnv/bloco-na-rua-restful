using System.Security.Claims;
using BlocoNaRua.Data.Repositories;
using BlocoNaRua.Data.Repositories.Interfaces;
using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace BlocoNaRua.Services.Implementations;

public class MemberIdentityService(
    IHttpContextAccessor httpContextAccessor,
    IMembersRepository membersRepository
) : IMemberIdentityService
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly IMembersRepository _membersRepository = membersRepository;

    public async Task<int> GetMemberIdAsync()
    {
        var member = await GetMemberAsync();
        return member?.Id
            ?? throw new UnauthorizedAccessException("Member not found from JWT claim.");
    }

    public async Task<MemberEntity?> GetMemberAsync()
    {
        var subClaim = _httpContextAccessor.HttpContext?.User.FindFirst("sub");
        if (subClaim is null)
            throw new UnauthorizedAccessException("Missing 'sub' claim in JWT.");

        if (!Guid.TryParse(subClaim.Value, out var uuid))
            throw new UnauthorizedAccessException("Invalid 'sub' claim format.");

        return await _membersRepository.GetByUuidAsync(uuid, CancellationToken.None);
    }
}