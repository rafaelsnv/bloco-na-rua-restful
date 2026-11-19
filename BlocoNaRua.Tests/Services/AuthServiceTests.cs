using BlocoNaRua.Data.Repositories.Interfaces;
using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Services.Implementations;
using BlocoNaRua.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace BlocoNaRua.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IMembersRepository> _membersRepositoryMock = new();
    private readonly Mock<ITokenService> _tokenServiceMock = new();
    private readonly IPasswordHasher<MemberEntity> _passwordHasher = new PasswordHasher<MemberEntity>();
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _service = new AuthService(_membersRepositoryMock.Object, _tokenServiceMock.Object, _passwordHasher);
        _membersRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<MemberEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _tokenServiceMock
            .Setup(t => t.IssueTokens(It.IsAny<MemberEntity>()))
            .Returns(("access", "refresh", 3600));
    }

    private MemberEntity CreatePasswordMember(string email, string password, bool isActive = true)
    {
        var member = new MemberEntity(1)
        {
            Name = "Test User",
            Email = email,
            Provider = "password",
            ProviderId = email,
            IsActive = isActive,
            EmailConfirmed = false,
            Uuid = Guid.NewGuid(),
        };
        member.PasswordHash = _passwordHasher.HashPassword(member, password);
        return member;
    }

    [Fact]
    public async Task SignupAsync_CreatesMember_WithPasswordProvider()
    {
        MemberEntity? added = null;
        _membersRepositoryMock
            .Setup(r => r.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((MemberEntity?)null);
        _membersRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<MemberEntity>(), It.IsAny<CancellationToken>()))
            .Callback<MemberEntity, CancellationToken>((entity, _) => added = entity)
            .Returns((MemberEntity entity, CancellationToken _) => Task.FromResult(entity));

        var result = await _service.SignupAsync(new SignupRequest("user@example.com", "password123", "Test User"));

        Assert.NotNull(result);
        Assert.NotNull(added);
        Assert.Equal("password", added!.Provider);
        Assert.Equal("user@example.com", added.ProviderId);
        Assert.Equal("user@example.com", added.Email);
        Assert.Equal("Test User", added.Name);
        Assert.False(added.EmailConfirmed);
        Assert.True(added.IsActive);
        Assert.NotEqual(Guid.Empty, added.Uuid);
        Assert.StartsWith("AQAAAA", added.PasswordHash);
        Assert.Equal("access", result!.Value.AccessToken);
        Assert.Equal("refresh", result.Value.RefreshToken);
        Assert.Equal(3600, result.Value.ExpiresIn);
    }

    [Fact]
    public async Task SignupAsync_ReturnsNull_WhenEmailIsAlreadyRegistered()
    {
        var existing = CreatePasswordMember("user@example.com", "password123");
        _membersRepositoryMock
            .Setup(r => r.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var result = await _service.SignupAsync(new SignupRequest("user@example.com", "password123", "Test User"));

        Assert.Null(result);
        _membersRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<MemberEntity>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SignupAsync_NormalizesEmailToLowerCase()
    {
        MemberEntity? added = null;
        _membersRepositoryMock
            .Setup(r => r.GetByEmailAsync("foo@bar.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((MemberEntity?)null);
        _membersRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<MemberEntity>(), It.IsAny<CancellationToken>()))
            .Callback<MemberEntity, CancellationToken>((entity, _) => added = entity)
            .Returns((MemberEntity entity, CancellationToken _) => Task.FromResult(entity));

        await _service.SignupAsync(new SignupRequest("Foo@Bar.com", "password123", "Test User"));

        Assert.NotNull(added);
        Assert.Equal("foo@bar.com", added!.Email);
        Assert.Equal("foo@bar.com", added.ProviderId);
    }

    [Fact]
    public async Task LoginAsync_ReturnsTokens_WhenPasswordMatches()
    {
        var member = CreatePasswordMember("user@example.com", "password123");
        _membersRepositoryMock
            .Setup(r => r.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(member);

        var result = await _service.LoginAsync(new LoginRequest("user@example.com", "password123"));

        Assert.Equal("access", result.AccessToken);
        Assert.Equal("refresh", result.RefreshToken);
        Assert.Equal(3600, result.ExpiresIn);
        Assert.Equal("bearer", result.TokenType);
        Assert.Equal(member.Uuid, result.UserId);
    }

    [Fact]
    public async Task LoginAsync_ThrowsUnauthorizedAccessException_WhenPasswordIsWrong()
    {
        var member = CreatePasswordMember("user@example.com", "password123");
        _membersRepositoryMock
            .Setup(r => r.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(member);

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.LoginAsync(new LoginRequest("user@example.com", "wrong-password")));

        Assert.Equal("Invalid credentials", ex.Message);
    }

    [Fact]
    public async Task LoginAsync_ThrowsUnauthorizedAccessException_WhenEmailIsUnknown()
    {
        _membersRepositoryMock
            .Setup(r => r.GetByEmailAsync("nobody@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((MemberEntity?)null);

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.LoginAsync(new LoginRequest("nobody@example.com", "password123")));

        Assert.Equal("Invalid credentials", ex.Message);
    }

    [Fact]
    public async Task LoginAsync_ThrowsUnauthorizedAccessException_WhenProviderIsNotPassword()
    {
        var member = new MemberEntity(1)
        {
            Name = "Google User",
            Email = "user@example.com",
            Provider = "google",
            ProviderId = "google-sub-789",
            Uuid = Guid.NewGuid(),
        };
        _membersRepositoryMock
            .Setup(r => r.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(member);

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.LoginAsync(new LoginRequest("user@example.com", "password123")));

        Assert.Equal("Invalid credentials", ex.Message);
    }

    [Fact]
    public async Task LoginAsync_ThrowsUnauthorizedAccessException_WhenMemberIsInactive()
    {
        var member = CreatePasswordMember("user@example.com", "password123", isActive: false);
        _membersRepositoryMock
            .Setup(r => r.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(member);

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.LoginAsync(new LoginRequest("user@example.com", "password123")));

        Assert.Equal("Member account is inactive.", ex.Message);
    }

    [Fact]
    public async Task ProcessGoogleCallbackAsync_CreatesMember_WhenProviderIsUnknown()
    {
        var providerId = "google-sub-123";
        MemberEntity? added = null;

        _membersRepositoryMock
            .Setup(r => r.GetByProviderAsync("google", providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MemberEntity?)null);
        _membersRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<MemberEntity>(), It.IsAny<CancellationToken>()))
            .Callback<MemberEntity, CancellationToken>((entity, _) => added = entity)
            .Returns((MemberEntity entity, CancellationToken _) => Task.FromResult(entity));

        var result = await _service.ProcessGoogleCallbackAsync(
            "Test User", "test@example.com", providerId, CancellationToken.None, "https://pic");

        Assert.NotNull(added);
        Assert.Equal("google", added!.Provider);
        Assert.Equal(providerId, added.ProviderId);
        Assert.Equal("test@example.com", added.Email);
        Assert.Equal("https://pic", added.ProfileImage);
        Assert.True(added.IsActive);
        Assert.NotEqual(Guid.Empty, added.Uuid);
        Assert.Equal("access", result.AccessToken);
        Assert.Equal("refresh", result.RefreshToken);
        Assert.Equal(3600, result.ExpiresIn);
    }

    [Fact]
    public async Task ProcessGoogleCallbackAsync_ReusesMember_WhenProviderIsKnown()
    {
        var providerId = "google-sub-456";
        var member = new MemberEntity(1)
        {
            Name = "Existing",
            Email = "existing@example.com",
            Provider = "google",
            ProviderId = providerId,
            Uuid = Guid.NewGuid(),
        };

        _membersRepositoryMock
            .Setup(r => r.GetByProviderAsync("google", providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(member);
        _membersRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<MemberEntity>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("member must not be created twice"));

        var result = await _service.ProcessGoogleCallbackAsync("Existing", "existing@example.com", providerId);

        Assert.Equal("access", result.AccessToken);
        Assert.Equal("refresh", result.RefreshToken);
    }

    [Fact]
    public async Task RevokeRefreshTokenAsync_ClearsRefreshToken()
    {
        var uuid = Guid.NewGuid();
        var member = new MemberEntity(1) { Uuid = uuid, RefreshToken = "token" };

        _membersRepositoryMock
            .Setup(r => r.GetByUuidAsync(uuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(member);

        await _service.RevokeRefreshTokenAsync(uuid);

        // Clearing the token is delegated to the token service; the service persists the member.
        _tokenServiceMock.Verify(t => t.RevokeRefreshToken(member), Times.Once);
        _membersRepositoryMock.Verify(r => r.UpdateAsync(member, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RevokeRefreshTokenAsync_DoesNothing_WhenMemberNotFound()
    {
        var uuid = Guid.NewGuid();

        _membersRepositoryMock
            .Setup(r => r.GetByUuidAsync(uuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MemberEntity?)null);

        await _service.RevokeRefreshTokenAsync(uuid);

        _tokenServiceMock.Verify(t => t.RevokeRefreshToken(It.IsAny<MemberEntity>()), Times.Never);
        _membersRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<MemberEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
