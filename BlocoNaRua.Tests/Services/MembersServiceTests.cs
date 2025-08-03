using BlocoNaRua.Data.Repositories.Interfaces;
using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Services.Implementations;

namespace BlocoNaRua.Tests.Services;

public class MembersServiceTests
{
    private readonly Mock<IMembersRepository> _repositoryMock;
    private readonly MembersService _service;

    public MembersServiceTests()
    {
        _repositoryMock = new Mock<IMembersRepository>();
        _service = new MembersService(_repositoryMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllMembers()
    {
        // Arrange
        var members = new List<MemberEntity>
        {
            new(1, "Member 1", "member1@test.com", "111", "img1.jpg"),
            new(2, "Member 2", "member2@test.com", "222", "img2.jpg")
        };
        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(members);

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
        var member = new MemberEntity(1, "Test Member", "test@test.com", "123", "img.jpg");
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(member);

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
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((MemberEntity?)null);

        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateMember()
    {
        // Arrange
        var newMember = new MemberEntity(0, "New Member", "new@test.com", "456", "new.jpg");
        var createdMember = new MemberEntity(1, "New Member", "new@test.com", "456", "new.jpg");
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<MemberEntity>())).ReturnsAsync(createdMember);

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
        var existingMember = new MemberEntity(1, "Old Name", "old@test.com", "123", "old.jpg");
        var updatedModel = new MemberEntity(1, "Updated Name", "updated@test.com", "321", "updated.jpg");

        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingMember);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<MemberEntity>())).ReturnsAsync(true);

        // Act
        var result = await _service.UpdateAsync(1, 1, updatedModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Name", result.Name);
        _repositoryMock.Verify(r => r.UpdateAsync(It.Is<MemberEntity>(m => m.Name == "Updated Name")), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowUnauthorized_WhenRequesterIsNotTarget()
    {
        // Arrange
        var updatedModel = new MemberEntity(1, "Updated Name", "updated@test.com", "321", "updated.jpg");

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.UpdateAsync(1, 2, updatedModel));
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnNull_WhenMemberDoesNotExist()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((MemberEntity?)null);
        var memberModel = new MemberEntity(999, "Non Existent", "none@test.com", "000", "none.jpg");

        // Act
        var result = await _service.UpdateAsync(999, 999, memberModel);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteMember_WhenRequesterIsTarget()
    {
        // Arrange
        var member = new MemberEntity(1, "Test Member", "test@test.com", "123", "img.jpg");
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(member);
        _repositoryMock.Setup(r => r.DeleteAsync(member)).ReturnsAsync(true);

        // Act
        var result = await _service.DeleteAsync(1, 1);

        // Assert
        Assert.True(result);
        _repositoryMock.Verify(r => r.DeleteAsync(member), Times.Once);
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
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((MemberEntity?)null);

        // Act
        var result = await _service.DeleteAsync(999, 999);

        // Assert
        Assert.False(result);
    }
}
