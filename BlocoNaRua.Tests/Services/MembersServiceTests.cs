using BlocoNaRua.Data.Repositories.Interfaces;
using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Domain.Enums;
using BlocoNaRua.Services.Implementations;
using BlocoNaRua.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace BlocoNaRua.Tests.Services;

public class MembersServiceTests
{
    private readonly Mock<IMembersRepository> _repositoryMock;
    private readonly Mock<ICarnivalBlockMembersRepository> _carnivalBlockMembersRepositoryMock;
    private readonly Mock<IMeetingsRepository> _meetingsRepositoryMock;
    private readonly Mock<IMemoryCache> _cacheMock;
    private readonly MembersService _service;

    public MembersServiceTests()
    {
        _repositoryMock = new Mock<IMembersRepository>();
        _carnivalBlockMembersRepositoryMock = new Mock<ICarnivalBlockMembersRepository>();
        _meetingsRepositoryMock = new Mock<IMeetingsRepository>();
        _cacheMock = new Mock<IMemoryCache>();
        _service = new MembersService(
            _repositoryMock.Object,
            _carnivalBlockMembersRepositoryMock.Object,
            _meetingsRepositoryMock.Object,
            _cacheMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllMembers()
    {
        // Arrange
        var members = new List<MemberEntity>
        {
            new(1, "Member 1", "member1@test.com", "111", "img1.jpg", new Guid()),
            new(2, "Member 2", "member2@test.com", "222", "img2.jpg", new Guid())
        };
        _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(members);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnMember_WhenMemberExists()
    {
        // Arrange
        var member = new MemberEntity(1, "Test Member", "test@test.com", "123", "img.jpg", new Guid());
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(member);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenMemberDoesNotExist()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MemberEntity?)null);

        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateMember()
    {
        // Arrange
        var newMember = new MemberEntity(0, "New Member", "new@test.com", "456", "new.jpg", new Guid());
        var createdMember = new MemberEntity(1, "New Member", "new@test.com", "456", "new.jpg", newMember.Uuid);
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<MemberEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdMember);

        // Act
        var result = await _service.CreateAsync(newMember);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("New Member", result.Name);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateMember_WhenRequesterIsTarget()
    {
        // Arrange
        var existingMember = new MemberEntity(1, "Old Name", "old@test.com", "123", "old.jpg", new Guid());
        var updatedModel = new MemberEntity(1, "Updated Name", "updated@test.com", "321", "updated.jpg", existingMember.Uuid);

        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingMember);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<MemberEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.UpdateAsync(1, 1, updatedModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Name", result.Name);
        _repositoryMock.Verify(r => r.UpdateAsync(It.Is<MemberEntity>(m => m.Name == "Updated Name"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowUnauthorized_WhenRequesterIsNotTarget()
    {
        // Arrange
        var updatedModel = new MemberEntity(1, "Updated Name", "updated@test.com", "321", "updated.jpg", new Guid());

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.UpdateAsync(1, 2, updatedModel));
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnNull_WhenMemberDoesNotExist()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MemberEntity?)null);
        var memberModel = new MemberEntity(999, "Non Existent", "none@test.com", "000", "none.jpg", new Guid());

        // Act
        var result = await _service.UpdateAsync(999, 999, memberModel);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteMember_WhenRequesterIsTarget()
    {
        // Arrange
        var member = new MemberEntity(1, "Test Member", "test@test.com", "123", "img.jpg", new Guid());
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(member);
        _repositoryMock.Setup(r => r.DeleteAsync(It.IsAny<MemberEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteAsync(1, 1);

        // Assert
        Assert.True(result);
        _repositoryMock.Verify(r => r.DeleteAsync(It.Is<MemberEntity>(m => m.Id == 1), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowUnauthorized_WhenRequesterIsNotTarget()
    {
        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.DeleteAsync(1, 2));
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenMemberDoesNotExist()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MemberEntity?)null);

        // Act
        var result = await _service.DeleteAsync(999, 999);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetByUuidAsync_ReturnsCachedMember_WhenMemberInCache()
    {
        // Arrange
        var uuid = Guid.NewGuid();
        var cachedMember = new MemberEntity(1, "Cached Member", "cached@test.com", "123", "img.jpg", uuid);
        object? cachedValue = cachedMember;
        _cacheMock.Setup(c => c.TryGetValue($"Member_{uuid}", out cachedValue)).Returns(true);

        // Act
        var result = await _service.GetByUuidAsync(uuid);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Cached Member", result.Name);
        _repositoryMock.Verify(r => r.GetByUuidAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetByUuidAsync_ReturnsFromRepository_WhenNotInCache()
    {
        // Arrange
        var uuid = Guid.NewGuid();
        var member = new MemberEntity(1, "DB Member", "db@test.com", "456", "db.jpg", uuid);
        object? cachedValue = null;
        _cacheMock.Setup(c => c.TryGetValue($"Member_{uuid}", out cachedValue)).Returns(false);
        _repositoryMock.Setup(r => r.GetByUuidAsync(uuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(member);

        // ponytail: extension methods SetValue/SetAbsoluteExpiration can't be mocked; skip inner verification
        // Use a lenient mock for ICacheEntry - the extension methods will set properties on it
        var cacheEntryMock = new Mock<ICacheEntry>(MockBehavior.Loose);
        cacheEntryMock.Setup(e => e.Dispose());
        _cacheMock.Setup(c => c.CreateEntry(It.IsAny<object>())).Returns(cacheEntryMock.Object);

        // Act
        var result = await _service.GetByUuidAsync(uuid);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("DB Member", result.Name);
        _repositoryMock.Verify(r => r.GetByUuidAsync(uuid, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByUuidAsync_ReturnsNull_WhenMemberDoesNotExist()
    {
        // Arrange
        var uuid = Guid.NewGuid();
        object? cachedValue = null;
        _cacheMock.Setup(c => c.TryGetValue($"Member_{uuid}", out cachedValue)).Returns(false);
        _repositoryMock.Setup(r => r.GetByUuidAsync(uuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MemberEntity?)null);

        // Act
        var result = await _service.GetByUuidAsync(uuid);

        // Assert
        Assert.Null(result);
        _repositoryMock.Verify(r => r.GetByUuidAsync(uuid, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetMemberBlocksAsync_ReturnsBlocks_WhenMemberHasBlocks()
    {
        // Arrange
        var memberId = 1;
        var blocks = new List<CarnivalBlockMembersEntity>
        {
            new CarnivalBlockMembersEntity(1, 10, memberId, RolesEnum.Member),
            new CarnivalBlockMembersEntity(2, 20, memberId, RolesEnum.Manager)
        };
        _carnivalBlockMembersRepositoryMock.Setup(r => r.GetByMemberIdAsync(memberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(blocks);

        // Act
        var result = await _service.GetMemberBlocksAsync(memberId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetMemberBlocksAsync_ReturnsEmptyList_WhenMemberHasNoBlocks()
    {
        // Arrange
        var memberId = 999;
        _carnivalBlockMembersRepositoryMock.Setup(r => r.GetByMemberIdAsync(memberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CarnivalBlockMembersEntity>());

        // Act
        var result = await _service.GetMemberBlocksAsync(memberId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetMemberMeetingsAsync_ReturnsMeetings_WhenMemberHasBlocks()
    {
        // Arrange
        var memberId = 1;
        var blockIds = new List<int> { 10, 20 };
        var blocks = new List<CarnivalBlockMembersEntity>
        {
            new CarnivalBlockMembersEntity(1, blockIds[0], memberId, RolesEnum.Member),
            new CarnivalBlockMembersEntity(2, blockIds[1], memberId, RolesEnum.Member)
        };
        var meetings = new List<MeetingEntity>
        {
            new MeetingEntity(1, "Meeting 1", "Desc 1", "Location 1", "M1", DateTime.Now, blockIds[0]),
            new MeetingEntity(2, "Meeting 2", "Desc 2", "Location 2", "M2", DateTime.Now, blockIds[1])
        };
        _carnivalBlockMembersRepositoryMock.Setup(r => r.GetByMemberIdAsync(memberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(blocks);
        _meetingsRepositoryMock.Setup(r => r.GetByBlockIdsAsync(blockIds, It.IsAny<CancellationToken>()))
            .ReturnsAsync(meetings);

        // Act
        var result = await _service.GetMemberMeetingsAsync(memberId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetMemberMeetingsAsync_ReturnsEmptyList_WhenMemberHasNoBlocks()
    {
        // Arrange
        var memberId = 999;
        _carnivalBlockMembersRepositoryMock.Setup(r => r.GetByMemberIdAsync(memberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CarnivalBlockMembersEntity>());

        // Act
        var result = await _service.GetMemberMeetingsAsync(memberId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}
