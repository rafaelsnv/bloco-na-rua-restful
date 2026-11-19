using BlocoNaRua.Data.Repositories.Interfaces;
using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace BlocoNaRua.Services.Implementations;

/// <summary>
/// API owned auth (plan §1/§3). No Supabase HTTP calls: members are upserted through the
/// members repository and tokens come from <see cref="ITokenService"/>.
/// </summary>
public class AuthService(
    IMembersRepository membersRepository,
    ITokenService tokenService,
    IPasswordHasher<MemberEntity> passwordHasher) : IAuthService
{
    private const string GoogleProvider = "google";
    private const string PasswordProvider = "password";
    private const string InvalidCredentialsMessage = "Invalid credentials";
    private const string InactiveMemberMessage = "Member account is inactive.";

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var email = request.Email.ToLowerInvariant();
        var member = await membersRepository.GetByEmailAsync(email, CancellationToken.None);

        // Neutral failure: an unknown email, a Google account and a missing hash all look the same
        // to the caller, so the response cannot be used to enumerate accounts.
        if (member is null
            || !string.Equals(member.Provider, PasswordProvider, StringComparison.Ordinal)
            || string.IsNullOrEmpty(member.PasswordHash))
        {
            throw new UnauthorizedAccessException(InvalidCredentialsMessage);
        }

        if (!member.IsActive)
        {
            throw new UnauthorizedAccessException(InactiveMemberMessage);
        }

        var verification = passwordHasher.VerifyHashedPassword(member, member.PasswordHash, request.Password);
        if (verification == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedAccessException(InvalidCredentialsMessage);
        }

        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            // The stored hash uses an outdated algorithm: replace it with the current one.
            member.PasswordHash = passwordHasher.HashPassword(member, request.Password);
        }

        var (accessToken, refreshToken, expiresIn) = tokenService.IssueTokens(member);
        // Persists the (rehashed) password and the new refresh token in one round trip.
        await membersRepository.UpdateAsync(member, CancellationToken.None);

        return new LoginResponse(accessToken, refreshToken, expiresIn, "bearer", member.Uuid);
    }

    public async Task<(string AccessToken, string RefreshToken, int ExpiresIn)?> SignupAsync(
        SignupRequest request,
        CancellationToken ct = default)
    {
        var email = request.Email.ToLowerInvariant();

        // A member with this email already exists: the caller answers 409.
        var existing = await membersRepository.GetByEmailAsync(email, ct);
        if (existing is not null)
        {
            return null;
        }

        var member = new MemberEntity(0)
        {
            Name = request.Name,
            Email = email,
            Provider = PasswordProvider,
            // provider_id is the lower cased email, so it matches the unique provider index.
            ProviderId = email,
            ProfileImage = string.Empty,
            IsActive = true,
            EmailConfirmed = false,
            Uuid = Guid.NewGuid(),
        };
        member.PasswordHash = passwordHasher.HashPassword(member, request.Password);

        await membersRepository.AddAsync(member, ct);

        var tokens = tokenService.IssueTokens(member);
        await membersRepository.UpdateAsync(member, ct);
        return tokens;
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
