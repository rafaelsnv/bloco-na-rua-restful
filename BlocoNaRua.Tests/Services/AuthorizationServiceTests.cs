using BlocoNaRua.Data.Repositories.Interfaces;
using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Domain.Enums;
using BlocoNaRua.Services.Implementations;

namespace BlocoNaRua.Tests.Services;

public class AuthorizationServiceTests
{
    private readonly Mock<ICarnivalBlocksRepository> _carnivalBlocksRepositoryMock;
    private readonly Mock<ICarnivalBlockMembersRepository> _carnivalBlockMembersRepositoryMock;
    private readonly AuthorizationService _service;

    public AuthorizationServiceTests()
    {
        _carnivalBlocksRepositoryMock = new Mock<ICarnivalBlocksRepository>();
        _carnivalBlockMembersRepositoryMock = new Mock<ICarnivalBlockMembersRepository>();
        _service = new AuthorizationService(
            _carnivalBlocksRepositoryMock.Object,
            _carnivalBlockMembersRepositoryMock.Object
        );
    }

    [Fact]
    public async Task GetMemberRole_ReturnsOwner_WhenMemberIsOwner()
    {
        // Arrange
        var carnivalBlockId = 1;
        var ownerId = 10;
        var carnivalBlock = new CarnivalBlockEntity(carnivalBlockId, ownerId, "Test Block", "code", "mgrcode", "img.jpg");

        _carnivalBlocksRepositoryMock.Setup(r => r.GetByIdAsync(carnivalBlockId)).ReturnsAsync(carnivalBlock);

        // Act
        var result = await _service.GetMemberRole(carnivalBlockId, ownerId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(RolesEnum.Owner, result);
    }

    [Fact]
    public async Task GetMemberRole_ReturnsMemberRole_WhenMemberIsNotOwner()
    {
        // Arrange
        var carnivalBlockId = 1;
        var ownerId = 10;
        var memberId = 20;
        var carnivalBlock = new CarnivalBlockEntity(carnivalBlockId, ownerId, "Test Block", "code", "mgrcode", "img.jpg");

        _carnivalBlocksRepositoryMock.Setup(r => r.GetByIdAsync(carnivalBlockId)).ReturnsAsync(carnivalBlock);
        _carnivalBlockMembersRepositoryMock.Setup(r => r.GetMemberRole(carnivalBlockId, memberId)).ReturnsAsync(RolesEnum.Manager);

        // Act
        var result = await _service.GetMemberRole(carnivalBlockId, memberId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(RolesEnum.Manager, result);
    }

    [Fact]
    public async Task GetMemberRole_ThrowsKeyNotFoundException_WhenCarnivalBlockDoesNotExist()
    {
        // Arrange
        var carnivalBlockId = 999;
        _carnivalBlocksRepositoryMock.Setup(r => r.GetByIdAsync(carnivalBlockId)).ReturnsAsync((CarnivalBlockEntity?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetMemberRole(carnivalBlockId, 1));
    }
}
