using BlocoNaRua.Data.Context;
using BlocoNaRua.Data.Repositories;
using BlocoNaRua.Data.Repositories.Interfaces;
using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Domain.Enums;
using BlocoNaRua.Services.Implementations;
using BlocoNaRua.Tests.Helpers;

namespace BlocoNaRua.Tests.Services;

public class CarnivalBlockServiceTests
{
    private readonly AppDbContext _contextMock;
    private readonly ICarnivalBlocksRepository _carnivalBlocksRepository;
    private readonly Mock<IMembersRepository> _membersRepositoryMock;
    private readonly ICarnivalBlockMembersRepository _carnivalBlockMembersRepository;
    private readonly CarnivalBlockService _carnivalBlockService;

    public CarnivalBlockServiceTests()
    {
        _contextMock = TestDbContextFactory.GetContext(Guid.NewGuid().ToString());
        _membersRepositoryMock = new Mock<IMembersRepository>();
        _carnivalBlocksRepository = new CarnivalBlocksRepository(_contextMock);
        _carnivalBlockMembersRepository = new CarnivalBlockMembersRepository
        (
            _contextMock,
            _membersRepositoryMock.Object,
            _carnivalBlocksRepository
        );
        _carnivalBlockService = new CarnivalBlockService
        (
            _carnivalBlocksRepository,
            _carnivalBlockMembersRepository
        );
    }

    private async Task AddData(int blockId, int ownerId, string blockName, int memberId, RolesEnum role)
    {
        _membersRepositoryMock
            .Setup(r => r.GetByIdAsync(ownerId))
            .ReturnsAsync(new MemberEntity(
                id: ownerId,
                name: "Owner Member",
                email: "owner@test.com",
                phone: "1234567890",
                profileImage: "profile_image.jpg"
            ));

        _membersRepositoryMock
            .Setup(r => r.GetByIdAsync(memberId))
            .ReturnsAsync(new MemberEntity(
                id: memberId,
                name: "Test Member",
                email: "test@test.com",
                phone: "1234567890",
                profileImage: "profile_image.jpg"
            ));

        var carnivalBlock = new CarnivalBlockEntity(
            id: blockId,
            ownerId: ownerId,
            name: blockName,
            inviteCode: "test",
            managersInviteCode: "test",
            carnivalBlockImage: "image.jpg"
        );
        await _carnivalBlocksRepository.AddAsync(carnivalBlock);

        var carnivalBlockMember = new CarnivalBlockMembersEntity(
            id: 0,
            carnivalBlockId: blockId,
            memberId: memberId,
            role: role
        );
        await _carnivalBlockMembersRepository.AddAsync(carnivalBlockMember);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllCarnivalBlocks()
    {
        // Arrange
        await AddData(1, 101, "Block 1", 101, RolesEnum.Owner);
        await AddData(2, 201, "Block 2", 202, RolesEnum.Manager);

        // Act
        var result = await _carnivalBlockService.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCarnivalBlock_WhenBlockExists()
    {
        // Arrange
        await AddData(1, 101, "Block 1", 101, RolesEnum.Owner);

        // Act
        var result = await _carnivalBlockService.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenBlockDoesNotExist()
    {
        // Act
        var result = await _carnivalBlockService.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateCarnivalBlock()
    {
        // Arrange
        var newBlock = new CarnivalBlockEntity(
            id: 0,
            ownerId: 1,
            name: "New Block",
            inviteCode: "test",
            managersInviteCode: "test",
            carnivalBlockImage: "image.jpg"
        );

        // Act
        var result = await _carnivalBlockService.CreateAsync(newBlock);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(0, result.Id);
        Assert.Equal("New Block", result.Name);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateCarnivalBlock_WhenMemberIsOwner()
    {
        // Arrange
        await AddData(1, 101, "Block 1", 101, RolesEnum.Owner);
        var updatedModel = new CarnivalBlockEntity(
            id: 1,
            ownerId: 101,
            name: "Updated Block 1",
            inviteCode: "test",
            managersInviteCode: "test",
            carnivalBlockImage: "updated_image.jpg"
        );

        // Act
        var result = await _carnivalBlockService.UpdateAsync(1, 101, updatedModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Block 1", result.Name);
        Assert.Equal("updated_image.jpg", result.CarnivalBlockImage);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateCarnivalBlock_WhenMemberIsManager()
    {
        // Arrange
        await AddData(2, 201, "Block 2", 202, RolesEnum.Manager);
        var updatedModel = new CarnivalBlockEntity(
            id: 2,
            ownerId: 201,
            name: "Updated Block 2",
            inviteCode: "test",
            managersInviteCode: "test",
            carnivalBlockImage: "updated_image.jpg"
        );

        // Act
        var result = await _carnivalBlockService.UpdateAsync(2, 202, updatedModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Block 2", result.Name);
        Assert.Equal("updated_image.jpg", result.CarnivalBlockImage);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowUnauthorizedAccessException_WhenMemberIsRegularMember()
    {
        // Arrange
        await AddData(3, 301, "Block 3", 302, RolesEnum.Member);
        var updatedModel = new CarnivalBlockEntity(
            id: 3,
            ownerId: 301,
            name: "Updated Block 3",
            inviteCode: "test",
            managersInviteCode: "test",
            carnivalBlockImage: "updated_image.jpg"
        );

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _carnivalBlockService.UpdateAsync(3, 302, updatedModel));
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnNull_WhenCarnivalBlockDoesNotExist()
    {
        // Arrange
        var updatedModel = new CarnivalBlockEntity(
            id: 999,
            ownerId: 1,
            name: "Non Existent Block",
            inviteCode: "test",
            managersInviteCode: "test",
            carnivalBlockImage: "image.jpg"
        );

        // Act
        var result = await _carnivalBlockService.UpdateAsync(999, 1, updatedModel);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteCarnivalBlock_WhenMemberIsOwner()
    {
        // Arrange
        await AddData(4, 401, "Block 4", 401, RolesEnum.Owner);

        // Act
        var result = await _carnivalBlockService.DeleteAsync(4, 401);

        // Assert
        Assert.True(result);
        var deletedBlock = await _carnivalBlocksRepository.GetByIdAsync(4);
        Assert.Null(deletedBlock);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowUnauthorizedAccessException_WhenMemberIsManager()
    {
        // Arrange
        await AddData(5, 501, "Block 5", 502, RolesEnum.Manager);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _carnivalBlockService.DeleteAsync(5, 502));
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowUnauthorizedAccessException_WhenMemberIsRegularMember()
    {
        // Arrange
        await AddData(6, 601, "Block 6", 602, RolesEnum.Member);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _carnivalBlockService.DeleteAsync(6, 602));
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenCarnivalBlockDoesNotExist()
    {
        // Act
        var result = await _carnivalBlockService.DeleteAsync(999, 1);

        // Assert
        Assert.False(result);
    }
}
