using BlocoNaRua.Data.Repositories.Interfaces;
using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Services.Interfaces;

namespace BlocoNaRua.Services.Implementations;

/// <summary>
/// API owned auth (plan §1/§3). No Supabase HTTP calls: members are upserted through the
/// members repository and tokens come from <see cref="ITokenService"/>.
/// </summary>
public class AuthService(IMembersRepository membersRepository, ITokenService tokenService) : IAuthService
{
    private const string GoogleProvider = "google";

    public Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        // Phase 2 (plan §7): the Supabase password grant is gone, no password check is performed.
        throw new NotImplementedException("Password login migrates in phase 2; use /auth/google");
    }

    public async Task<(string AccessToken, string RefreshToken, int ExpiresIn)> ProcessGoogleCallbackAsync(
        string displayName,
        string email,
        string providerId,
        CancellationToken ct = default,
        string? picture = null)
    {
        var existing = await membersRepository.GetByProviderAsync(GoogleProvider, providerId, ct);

        MemberEntity member;
        if (existing is null)
        {
            member = await membersRepository.AddAsync(
                new MemberEntity(0)
                {
                    Name = string.IsNullOrWhiteSpace(displayName) ? email : displayName,
                    Email = email,
                    Provider = GoogleProvider,
                    ProviderId = providerId,
                    ProfileImage = picture ?? string.Empty,
                    IsActive = true,
                    EmailConfirmed = true,
                    Uuid = Guid.NewGuid(),
                },
                ct);
        }
        else
        {
            if (!existing.IsActive)
            {
                throw new UnauthorizedAccessException("Member account is inactive.");
            }

            member = existing;

            // A Google account without a picture must not wipe an avatar set earlier.
            if (!string.IsNullOrWhiteSpace(picture) && string.IsNullOrWhiteSpace(member.ProfileImage))
            {
                member.ProfileImage = picture;
            }
        }

        var tokens = tokenService.IssueTokens(member);
        await membersRepository.UpdateAsync(member, ct);
        return tokens;
    }

    public async Task<(string AccessToken, string RefreshToken, int ExpiresIn)?> RotateRefreshTokenAsync(
        string refreshToken,
        CancellationToken ct = default)
    {
        var validated = await tokenService.ValidateRefreshToken(refreshToken);
        if (validated is null)
        {
            return null;
        }

        var member = validated.Value.Member;
        var tokens = tokenService.RotateRefreshToken(member);
        await membersRepository.UpdateAsync(member, ct);
        return tokens;
    }

    public async Task RevokeRefreshTokenAsync(Guid memberUuid, CancellationToken ct = default)
    {
        var member = await membersRepository.GetByUuidAsync(memberUuid, ct);
        if (member is null)
        {
            return;
        }

        tokenService.RevokeRefreshToken(member);
        await membersRepository.UpdateAsync(member, ct);
    }
}
