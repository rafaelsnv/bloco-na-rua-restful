using BlocoNaRua.Data.Repositories.Interfaces;
using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Services.Implementations;
using Moq;

namespace BlocoNaRua.Tests.Services;

public class AdminServiceTests
{
    private readonly Mock<IMembersRepository> _membersRepositoryMock;
    private readonly AdminService _service;

    public AdminServiceTests()
    {
        _membersRepositoryMock = new Mock<IMembersRepository>();
        _service = new AdminService(_membersRepositoryMock.Object);
    }

    [Fact]
    public async Task DeleteSignupAsync_ReturnsSuccess_WhenMemberExists()
    {
        // Arrange
        var uuid = Guid.NewGuid();
        var member = new MemberEntity(1) { Uuid = uuid };
        _membersRepositoryMock
            .Setup(r => r.GetByUuidIgnoreFiltersAsync(uuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(member);
        _membersRepositoryMock
            .Setup(r => r.HardDeleteAsync(member, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteSignupAsync(uuid);

        // Assert
        Assert.True(result.Deleted);
        Assert.Null(result.ErrorMessage);
        _membersRepositoryMock.Verify(r => r.HardDeleteAsync(member, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteSignupAsync_ReturnsNotDeleted_WhenMemberMissing()
    {
        // Arrange
        var uuid = Guid.NewGuid();
        _membersRepositoryMock
            .Setup(r => r.GetByUuidIgnoreFiltersAsync(uuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MemberEntity?)null);

        // Act
        var result = await _service.DeleteSignupAsync(uuid);

        // Assert
        Assert.False(result.Deleted);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteSignupAsync_ReturnsErrorMessage_WhenRepositoryThrows()
    {
        // Arrange
        var uuid = Guid.NewGuid();
        _membersRepositoryMock
            .Setup(r => r.GetByUuidIgnoreFiltersAsync(uuid, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("boom"));

        // Act
        var result = await _service.DeleteSignupAsync(uuid);

        // Assert
        Assert.False(result.Deleted);
        Assert.Equal("boom", result.ErrorMessage);
    }
}
