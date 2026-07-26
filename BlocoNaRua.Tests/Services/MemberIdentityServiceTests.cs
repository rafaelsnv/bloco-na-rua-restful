using System.Security.Claims;
using BlocoNaRua.Data.Repositories.Interfaces;
using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Services.Implementations;
using Microsoft.AspNetCore.Http;

namespace BlocoNaRua.Tests.Services;

public class MemberIdentityServiceTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<IMembersRepository> _membersRepositoryMock;
    private readonly MemberIdentityService _service;

    public MemberIdentityServiceTests()
    {
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _membersRepositoryMock = new Mock<IMembersRepository>();
        _service = new MemberIdentityService(_httpContextAccessorMock.Object, _membersRepositoryMock.Object);
    }

    [Fact]
    public async Task GetMemberAsync_ReturnsMember_WhenValidSubClaim()
    {
        // Arrange
        var memberUuid = Guid.NewGuid();
        var member = new MemberEntity(42, "Test Member", "test@test.com", "123", "img.jpg", memberUuid);

        var claimsIdentity = new ClaimsIdentity(new[]
        {
            new Claim("sub", memberUuid.ToString())
        });
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        var httpContext = new DefaultHttpContext { User = claimsPrincipal };
        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(httpContext);

        _membersRepositoryMock.Setup(r => r.GetByUuidAsync(memberUuid, It.IsAny<CancellationToken>())).ReturnsAsync(member);

        // Act
        var result = await _service.GetMemberAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(42, result.Id);
        Assert.Equal(memberUuid, result.Uuid);
    }

    [Fact]
    public async Task GetMemberAsync_ThrowsUnauthorized_WhenSubClaimMissing()
    {
        // Arrange
        var claimsIdentity = new ClaimsIdentity(Array.Empty<Claim>());
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        var httpContext = new DefaultHttpContext { User = claimsPrincipal };
        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(httpContext);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.GetMemberAsync());
    }

    [Fact]
    public async Task GetMemberAsync_ThrowsUnauthorized_WhenSubClaimInvalidGuid()
    {
        // Arrange
        var claimsIdentity = new ClaimsIdentity(new[]
        {
            new Claim("sub", "not-a-valid-guid")
        });
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        var httpContext = new DefaultHttpContext { User = claimsPrincipal };
        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(httpContext);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.GetMemberAsync());
    }

    [Fact]
    public async Task GetMemberIdAsync_ReturnsMemberId_WhenMemberExists()
    {
        // Arrange
        var memberUuid = Guid.NewGuid();
        var member = new MemberEntity(99, "Test Member", "test@test.com", "123", "img.jpg", memberUuid);

        var claimsIdentity = new ClaimsIdentity(new[]
        {
            new Claim("sub", memberUuid.ToString())
        });
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        var httpContext = new DefaultHttpContext { User = claimsPrincipal };
        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(httpContext);

        _membersRepositoryMock.Setup(r => r.GetByUuidAsync(memberUuid, It.IsAny<CancellationToken>())).ReturnsAsync(member);

        // Act
        var result = await _service.GetMemberIdAsync();

        // Assert
        Assert.Equal(99, result);
    }

    [Fact]
    public async Task GetMemberIdAsync_ThrowsUnauthorized_WhenMemberNotFound()
    {
        // Arrange
        var memberUuid = Guid.NewGuid();

        var claimsIdentity = new ClaimsIdentity(new[]
        {
            new Claim("sub", memberUuid.ToString())
        });
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        var httpContext = new DefaultHttpContext { User = claimsPrincipal };
        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(httpContext);

        _membersRepositoryMock.Setup(r => r.GetByUuidAsync(memberUuid, It.IsAny<CancellationToken>())).ReturnsAsync((MemberEntity?)null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.GetMemberIdAsync());
    }
}
