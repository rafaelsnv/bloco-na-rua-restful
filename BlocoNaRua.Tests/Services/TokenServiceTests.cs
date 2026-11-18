using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BlocoNaRua.Data.Context;
using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BlocoNaRua.Tests.Services;

public class TokenServiceTests
{
    private const string Issuer = "test-issuer";
    private const string Audience = "test-audience";

    // 32 bytes, base64 encoded: the same format the deployment uses.
    private const string SigningKey = "dGVzdC1zaWduaW5nLWtleS1mb3ItYm5yLXRlc3RzLTMyIQ==";

    private static IConfiguration BuildConfiguration(string? signingKey = SigningKey)
    {
        var values = new Dictionary<string, string?>
        {
            ["Auth:Issuer"] = Issuer,
            ["Auth:Audience"] = Audience,
        };

        if (signingKey is not null)
        {
            values["Auth:SigningKey"] = signingKey;
        }

        return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
    }

    private static AppDbContext BuildDbContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static TokenService CreateService(IConfiguration configuration, AppDbContext? dbContext = null) =>
        new(configuration, dbContext ?? BuildDbContext());

    private static MemberEntity CreateMember() => new(1)
    {
        Name = "Test Member",
        Email = "test@example.com",
        Uuid = Guid.NewGuid(),
        Provider = "google",
    };

    private static SymmetricSecurityKey BuildValidationKey(string signingKey)
    {
        var decoded = Convert.FromBase64String(signingKey);
        return new SymmetricSecurityKey(decoded);
    }

    [Fact]
    public void IssueTokens_ReturnsAccessTokenRefreshTokenAndExpiry()
    {
        var service = CreateService(BuildConfiguration());
        var member = CreateMember();

        var (accessToken, refreshToken, expiresIn) = service.IssueTokens(member);

        Assert.False(string.IsNullOrWhiteSpace(accessToken));
        Assert.False(string.IsNullOrWhiteSpace(refreshToken));
        Assert.True(expiresIn > 0);
        Assert.Equal(refreshToken, member.RefreshToken);
        Assert.NotNull(member.RefreshTokenExpiresAt);
    }

    [Fact]
    public void IssueTokens_AccessTokenValidatesWithExpectedClaims()
    {
        var service = CreateService(BuildConfiguration());
        var member = CreateMember();

        var (accessToken, _, _) = service.IssueTokens(member);

        var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };
        var principal = handler.ValidateToken(
            accessToken,
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = Issuer,
                ValidateAudience = true,
                ValidAudience = Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = BuildValidationKey(SigningKey),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
            },
            out _);

        Assert.Equal(member.Uuid.ToString(), principal.FindFirst("sub")?.Value);
        Assert.Equal(member.Email, principal.FindFirst("email")?.Value);
        Assert.Equal("google", principal.FindFirst("provider")?.Value);
        Assert.Equal("member", principal.FindFirst("role")?.Value);
    }

    [Fact]
    public void IssueTokens_Throws_WhenSigningKeyIsMissing()
    {
        var service = CreateService(BuildConfiguration(signingKey: null));

        var exception = Assert.Throws<InvalidOperationException>(() => service.IssueTokens(CreateMember()));
        Assert.Contains("Auth:SigningKey is missing", exception.Message);
    }

    [Fact]
    public void IssueTokens_Throws_WhenSigningKeyIsTooShort()
    {
        var service = CreateService(BuildConfiguration(signingKey: "too-short"));

        var exception = Assert.Throws<InvalidOperationException>(() => service.IssueTokens(CreateMember()));
        Assert.Contains("at least 32 bytes", exception.Message);
    }

    [Fact]
    public async Task ValidateRefreshToken_ReturnsNull_WhenTokenIsUnknown()
    {
        var dbContext = BuildDbContext();
        var service = CreateService(BuildConfiguration(), dbContext);

        var result = await service.ValidateRefreshToken("unknown-token");

        Assert.Null(result);
    }

    [Fact]
    public async Task ValidateRefreshToken_ReturnsMember_WhenTokenMatches()
    {
        var dbContext = BuildDbContext();
        var service = CreateService(BuildConfiguration(), dbContext);
        var member = CreateMember();

        var (_, refreshToken, _) = service.IssueTokens(member);
        dbContext.Members.Add(member);
        await dbContext.SaveChangesAsync();

        var result = await service.ValidateRefreshToken(refreshToken);

        Assert.NotNull(result);
        Assert.Equal(member.Id, result!.Value.Member.Id);
    }

    [Fact]
    public void RotateRefreshToken_ProducesDifferentRefreshToken()
    {
        var service = CreateService(BuildConfiguration());
        var member = CreateMember();

        var (_, firstRefreshToken, _) = service.IssueTokens(member);
        var (_, rotatedRefreshToken, _) = service.RotateRefreshToken(member);

        Assert.NotEqual(firstRefreshToken, rotatedRefreshToken);
        Assert.Equal(rotatedRefreshToken, member.RefreshToken);
    }

    [Fact]
    public async Task RotateRefreshToken_InvalidatesTheOldRefreshToken()
    {
        var dbContext = BuildDbContext();
        var service = CreateService(BuildConfiguration(), dbContext);
        var member = CreateMember();

        var (_, firstRefreshToken, _) = service.IssueTokens(member);
        dbContext.Members.Add(member);
        await dbContext.SaveChangesAsync();

        var (_, rotatedRefreshToken, _) = service.RotateRefreshToken(member);
        await dbContext.SaveChangesAsync();

        // The rotated token is the only valid one; the old one can no longer be replayed.
        Assert.NotNull(await service.ValidateRefreshToken(rotatedRefreshToken));
        Assert.Null(await service.ValidateRefreshToken(firstRefreshToken));
    }

    [Fact]
    public void RevokeRefreshToken_ClearsTokenAndExpiry()
    {
        var service = CreateService(BuildConfiguration());
        var member = CreateMember();

        service.IssueTokens(member);
        service.RevokeRefreshToken(member);

        Assert.Null(member.RefreshToken);
        Assert.Null(member.RefreshTokenExpiresAt);
    }
}
