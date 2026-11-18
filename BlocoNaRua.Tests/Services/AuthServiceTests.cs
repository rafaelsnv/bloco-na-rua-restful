using BlocoNaRua.Data.Repositories.Interfaces;
using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Services.Implementations;
using BlocoNaRua.Services.Interfaces;

namespace BlocoNaRua.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IMembersRepository> _membersRepositoryMock = new();
    private readonly Mock<ITokenService> _tokenServiceMock = new();
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _service = new AuthService(_membersRepositoryMock.Object, _tokenServiceMock.Object);
        _membersRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<MemberEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _tokenServiceMock
            .Setup(t => t.IssueTokens(It.IsAny<MemberEntity>()))
            .Returns(("access", "refresh", 3600));
    }

    [Fact]
    public async Task LoginAsync_ThrowsNotImplemented()
    {
        var request = new LoginRequest("test@example.com", "password123");

        await Assert.ThrowsAsync<NotImplementedException>(() => _service.LoginAsync(request));
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
