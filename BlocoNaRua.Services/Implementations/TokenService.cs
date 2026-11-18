using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BlocoNaRua.Data.Context;
using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BlocoNaRua.Services.Implementations;

/// <summary>
/// Self issued HS256 access tokens + opaque refresh tokens (plan §1/§3).
/// All configuration comes from the <c>Auth</c> section; a missing signing key is a loud failure,
/// never a baked-in default.
/// </summary>
public class TokenService(IConfiguration configuration, AppDbContext appDbContext) : ITokenService
{
    private const int MinimumSigningKeyBytes = 32;
    private const int RefreshTokenByteLength = 64;
    private const string DefaultIssuer = "bloconarua-api";
    private const string DefaultAudience = "bloconarua-api";
    private const int DefaultAccessTokenExpirationSeconds = 3600;
    private const int DefaultRefreshTokenExpirationDays = 30;
    private const string AdminRole = "admin";
    private const string MemberRole = "member";

    private readonly IConfiguration _configuration = configuration;
    private readonly AppDbContext _appDbContext = appDbContext;

    public (string AccessToken, string RefreshToken, int ExpiresIn) IssueTokens(MemberEntity member)
    {
        var expiresIn = GetAccessTokenExpirationSeconds();
        var accessToken = CreateAccessToken(member, expiresIn);
        var refreshToken = GenerateRefreshToken();

        member.RefreshToken = refreshToken;
        member.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(GetRefreshTokenExpirationDays());

        return (accessToken, refreshToken, expiresIn);
    }

    public (string AccessToken, string RefreshToken, int ExpiresIn) RotateRefreshToken(MemberEntity member)
    {
        // A fresh random token is generated on every call, so the new token never equals the old one.
        return IssueTokens(member);
    }

    public async Task<(MemberEntity Member, DateTime ExpiresAt)?> ValidateRefreshToken(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return null;
        }

        var member = await _appDbContext.Members
            .FirstOrDefaultAsync(m => m.RefreshToken == refreshToken);

        if (member is null || member.RefreshTokenExpiresAt is null)
        {
            return null;
        }

        var expiresAt = member.RefreshTokenExpiresAt.Value;
        if (expiresAt <= DateTime.UtcNow)
        {
            return null;
        }

        return (member, expiresAt);
    }

    public void RevokeRefreshToken(MemberEntity member)
    {
        member.RefreshToken = null;
        member.RefreshTokenExpiresAt = null;
    }

    private string CreateAccessToken(MemberEntity member, int expiresIn)
    {
        // Literal claim types — the API validates tokens with MapInboundClaims = false.
        var claims = new List<Claim>
        {
            new("sub", member.Uuid.ToString()),
            new("email", member.Email),
            new("provider", member.Provider ?? "password"),
            new("role", ResolveRole(member)),
        };

        var token = new JwtSecurityToken(
            issuer: GetIssuer(),
            audience: GetAudience(),
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddSeconds(expiresIn),
            signingCredentials: new SigningCredentials(GetSigningKey(), SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var bytes = new byte[RefreshTokenByteLength];
        RandomNumberGenerator.Fill(bytes);
        return Base64UrlEncoder.Encode(bytes);
    }

    private string ResolveRole(MemberEntity member)
    {
        // No admin marker exists on the member row yet: admins are the emails listed in
        // Auth:AdminEmails (comma separated). Task 07 may replace this with a real marker.
        var adminEmails = _configuration["Auth:AdminEmails"];
        if (string.IsNullOrWhiteSpace(adminEmails))
        {
            return MemberRole;
        }

        var isAdmin = adminEmails
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Any(email => string.Equals(email, member.Email, StringComparison.OrdinalIgnoreCase));

        return isAdmin ? AdminRole : MemberRole;
    }

    private string GetIssuer() => GetNonEmptySetting("Auth:Issuer") ?? DefaultIssuer;

    private string GetAudience() => GetNonEmptySetting("Auth:Audience") ?? DefaultAudience;

    private int GetAccessTokenExpirationSeconds() => GetPositiveIntSetting("Auth:AccessTokenExpirationSeconds", DefaultAccessTokenExpirationSeconds);

    private int GetRefreshTokenExpirationDays() => GetPositiveIntSetting("Auth:RefreshTokenExpirationDays", DefaultRefreshTokenExpirationDays);

    private string? GetNonEmptySetting(string key)
    {
        var value = _configuration[key];
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private int GetPositiveIntSetting(string key, int fallback)
    {
        var raw = _configuration[key];
        return int.TryParse(raw, out var value) && value > 0 ? value : fallback;
    }

    private SymmetricSecurityKey GetSigningKey()
    {
        var signingKey = _configuration["Auth:SigningKey"];
        if (string.IsNullOrWhiteSpace(signingKey))
        {
            throw new InvalidOperationException(
                "Auth:SigningKey is missing. Set the Auth__SigningKey environment variable to the secret used to sign the API tokens.");
        }

        var keyBytes = DecodeSigningKey(signingKey);
        if (keyBytes.Length < MinimumSigningKeyBytes)
        {
            throw new InvalidOperationException(
                $"Auth:SigningKey must decode to at least {MinimumSigningKeyBytes} bytes to sign tokens with HS256.");
        }

        return new SymmetricSecurityKey(keyBytes);
    }

    private static byte[] DecodeSigningKey(string signingKey)
    {
        // Deployment format (plan §5) is base64; a raw secret is accepted as a fallback.
        // The JWT validator has to use the very same bytes.
        if (TryDecodeBase64(signingKey, out var decoded) && decoded.Length >= MinimumSigningKeyBytes)
        {
            return decoded;
        }

        return Encoding.UTF8.GetBytes(signingKey);
    }

    private static bool TryDecodeBase64(string value, out byte[] decoded)
    {
        try
        {
            decoded = Convert.FromBase64String(value);
            return true;
        }
        catch (FormatException)
        {
            decoded = [];
            return false;
        }
    }
}
