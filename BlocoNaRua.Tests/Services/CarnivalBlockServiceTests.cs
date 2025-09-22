using BlocoNaRua.Data.Context;
using BlocoNaRua.Data.Repositories;
using BlocoNaRua.Data.Repositories.Interfaces;
using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Domain.Enums;
using BlocoNaRua.Services.Implementations;
using BlocoNaRua.Services.Interfaces;
using BlocoNaRua.Tests.Helpers;

namespace BlocoNaRua.Tests.Services;

public class CarnivalBlockServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly ICarnivalBlocksRepository _carnivalBlocksRepository;
    private readonly IMembersRepository _membersRepository;
    private readonly ICarnivalBlockMembersRepository _carnivalBlockMembersRepository;
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;
    private readonly CarnivalBlockService _carnivalBlockService;

    public CarnivalBlockServiceTests()
    {
        var dbName = Guid.NewGuid().ToString();
        _context = TestDbContextFactory.GetContext(dbName);
        _carnivalBlocksRepository = new CarnivalBlocksRepository(_context);
        _membersRepository = new MembersRepository(_context);
        _carnivalBlockMembersRepository = new CarnivalBlockMembersRepository(_context);
        _authorizationServiceMock = new Mock<IAuthorizationService>();
        _carnivalBlockService = new CarnivalBlockService
        (
            _carnivalBlocksRepository,
            _membersRepository,
            _authorizationServiceMock.Object
        );
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task AddData(int blockId, int ownerId, string blockName, int memberId, RolesEnum role)
    {
        if (await _membersRepository.GetByIdAsync(ownerId) is null)
            await _membersRepository.AddAsync(new MemberEntity(
                id: ownerId,
                name: "Owner Member",
                email: "owner@test.com",
                phone: "1234567890",
                profileImage: "profile_image.jpg",
                uuid: new Guid()
            ));

        if (ownerId != memberId && await _membersRepository.GetByIdAsync(memberId) is null)
        {
            await _membersRepository.AddAsync(new MemberEntity(
                id: memberId,
                name: "Test Member",
                email: "test@test.com",
                phone: "1234567890",
                profileImage: "profile_image.jpg",
                uuid: new Guid()
            ));
        }

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
        await _membersRepository.AddAsync(new MemberEntity(1, "owner", "owner@email.com", "123", "img", new Guid()));
        var newBlock = new CarnivalBlockEntity(0, 1, "New Block", "", "", "image.jpg");

        // Act
        var result = await _carnivalBlockService.CreateAsync(newBlock);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(0, result.Id);
        Assert.Equal("New Block", result.Name);
        Assert.False(string.IsNullOrEmpty(result.InviteCode));
        Assert.False(string.IsNullOrEmpty(result.ManagersInviteCode));
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
        _authorizationServiceMock.Setup(s => s.GetMemberRole(1, 101)).ReturnsAsync(RolesEnum.Owner);
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
        _authorizationServiceMock.Setup(s => s.GetMemberRole(2, 202)).ReturnsAsync(RolesEnum.Manager);
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
        _authorizationServiceMock.Setup(s => s.GetMemberRole(3, 302)).ReturnsAsync(RolesEnum.Member);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _carnivalBlockService.UpdateAsync(3, 302, updatedModel));
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteCarnivalBlock_WhenMemberIsOwner()
    {
        // Arrange
        await AddData(4, 401, "Block 4", 401, RolesEnum.Owner);

        // Act
        _authorizationServiceMock.Setup(s => s.GetMemberRole(4, 401)).ReturnsAsync(RolesEnum.Owner);
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
        _authorizationServiceMock.Setup(s => s.GetMemberRole(5, 502)).ReturnsAsync(RolesEnum.Manager);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _carnivalBlockService.DeleteAsync(5, 502));
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowUnauthorizedAccessException_WhenMemberIsRegularMember()
    {
        // Arrange
        await AddData(6, 601, "Block 6", 602, RolesEnum.Member);

        // Act & Assert
        _authorizationServiceMock.Setup(s => s.GetMemberRole(6, 602)).ReturnsAsync(RolesEnum.Member);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _carnivalBlockService.DeleteAsync(6, 602));
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowKeyNotFoundException_WhenCarnivalBlockDoesNotExist()
    {
        // Act & Assert
        _authorizationServiceMock.Setup(s => s.GetMemberRole(999, 1)).ThrowsAsync(new KeyNotFoundException());
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _carnivalBlockService.DeleteAsync(999, 1));
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowKeyNotFoundException_WhenOwnerDoesNotExist()
    {
        // Arrange
        var newBlock = new CarnivalBlockEntity(0, 999, "New Block", "", "", "image.jpg");

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _carnivalBlockService.CreateAsync(newBlock));
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowKeyNotFoundException_WhenCarnivalBlockDoesNotExist()
    {
        // Arrange
        var updatedModel = new CarnivalBlockEntity(999, 1, "Non Existent Block", "test", "test", "image.jpg");

        // Act & Assert
        _authorizationServiceMock.Setup(s => s.GetMemberRole(999, 1)).ThrowsAsync(new KeyNotFoundException());
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _carnivalBlockService.UpdateAsync(999, 1, updatedModel));
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowKeyNotFoundException_WhenMemberDoesNotExist()
    {
        // Arrange
        await AddData(1, 101, "Block 1", 101, RolesEnum.Owner);
        var updatedModel = new CarnivalBlockEntity(1, 101, "Updated Block 1", "test", "test", "updated_image.jpg");

        // Act & Assert
        _authorizationServiceMock.Setup(s => s.GetMemberRole(1, 999)).ThrowsAsync(new KeyNotFoundException());
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _carnivalBlockService.UpdateAsync(1, 999, updatedModel));
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowKeyNotFoundException_WhenMemberDoesNotExist()
    {
        // Arrange
        await AddData(4, 401, "Block 4", 401, RolesEnum.Owner);

        // Act & Assert
        _authorizationServiceMock.Setup(s => s.GetMemberRole(4, 999)).ThrowsAsync(new KeyNotFoundException());
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _carnivalBlockService.DeleteAsync(4, 999));
    }
}
