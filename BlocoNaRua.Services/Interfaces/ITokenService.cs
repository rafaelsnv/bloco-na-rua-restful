using BlocoNaRua.Domain.Entities;

namespace BlocoNaRua.Services.Interfaces;

/// <summary>
/// Issues the API's own tokens (plan §1/§3): a short lived HS256 access JWT and an opaque
/// refresh token stored on the member row.
///
/// Contract notes for callers:
/// <list type="bullet">
/// <item>Claim names are literal (<c>sub</c>, <c>email</c>, <c>provider</c>, <c>role</c>) — the JWT
/// validator must run with <c>MapInboundClaims = false</c>.</item>
/// <item>The refresh token secret is derived from <c>Auth:SigningKey</c>: base64 decoded when it
/// decodes to at least 32 bytes, otherwise the raw UTF-8 bytes of the configured value. The JWT
/// validator must use the exact same bytes.</item>
/// <item>Issuing/rotating only mutates the member entity; persisting it is the caller's job.</item>
/// </list>
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Issues an access token and a brand new opaque refresh token.
    /// The refresh token and its expiry are written to <paramref name="member"/>.
    /// </summary>
    (string AccessToken, string RefreshToken, int ExpiresIn) IssueTokens(MemberEntity member);

    /// <summary>
    /// Rotates the refresh token of <paramref name="member"/> (new, distinct token) and re-issues
    /// the access token. The caller persists the member afterwards.
    /// </summary>
    (string AccessToken, string RefreshToken, int ExpiresIn) RotateRefreshToken(MemberEntity member);

    /// <summary>
    /// Resolves the member that owns <paramref name="refreshToken"/>.
    /// Returns <c>null</c> when the token is unknown, revoked or expired.
    /// </summary>
    Task<(MemberEntity Member, DateTime ExpiresAt)?> ValidateRefreshToken(string refreshToken);

    /// <summary>Clears the refresh token of the member (logout / revocation).</summary>
    void RevokeRefreshToken(MemberEntity member);
}
