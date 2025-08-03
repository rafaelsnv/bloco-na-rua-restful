using BlocoNaRua.Data.Context;
using BlocoNaRua.Data.Repositories;
using BlocoNaRua.Data.Repositories.Interfaces;
using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Domain.Enums;
using BlocoNaRua.Services.Implementations;
using BlocoNaRua.Tests.Helpers;

namespace BlocoNaRua.Tests.Services;

public class CarnivalBlockMembersServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly ICarnivalBlockMembersRepository _carnivalBlockMembersRepository;
    private readonly IMembersRepository _membersRepository;
    private readonly ICarnivalBlocksRepository _carnivalBlocksRepository;
    private readonly CarnivalBlockMembersService _carnivalBlockMembersService;

    public CarnivalBlockMembersServiceTests()
    {
        var dbName = Guid.NewGuid().ToString();
        _context = TestDbContextFactory.GetContext(dbName);
        _carnivalBlockMembersRepository = new CarnivalBlockMembersRepository(_context);
        _membersRepository = new MembersRepository(_context);
        _carnivalBlocksRepository = new CarnivalBlocksRepository(_context);
        _carnivalBlockMembersService = new CarnivalBlockMembersService
        (
            _carnivalBlockMembersRepository,
            _membersRepository,
            _carnivalBlocksRepository
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
                profileImage: "profile_image.jpg"
            ));

        if (ownerId != memberId && await _membersRepository.GetByIdAsync(memberId) is null)
        {
            await _membersRepository.AddAsync(new MemberEntity(
                id: memberId,
                name: "Test Member",
                email: "test@test.com",
                phone: "1234567890",
                profileImage: "profile_image.jpg"
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
    public async Task GetAllAsync_ShouldReturnAllCarnivalBlockMembers()
    {
        // Arrange
        await AddData(1, 101, "Block 1", 101, RolesEnum.Owner);
        await AddData(2, 201, "Block 2", 202, RolesEnum.Manager);

        // Act
        var result = await _carnivalBlockMembersService.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCarnivalBlockMember_WhenBlockMemberExists()
    {
        // Arrange
        await AddData(1, 101, "Block 1", 101, RolesEnum.Owner);
        var blockMember = await _carnivalBlockMembersRepository.GetAllAsync();

        // Act
        var result = await _carnivalBlockMembersService.GetByIdAsync(blockMember.First().Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(blockMember.First().Id, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenBlockMemberDoesNotExist()
    {
        // Act
        var result = await _carnivalBlockMembersService.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateCarnivalBlockMember()
    {
        // Arrange
        await _membersRepository.AddAsync(new MemberEntity(1, "member", "member@email.com", "123", "img"));
        await _carnivalBlocksRepository.AddAsync(new CarnivalBlockEntity(1, 1, "block", "test", "test", "img"));
        var newBlockMember = new CarnivalBlockMembersEntity(0, 1, 1, RolesEnum.Member);

        // Act
        await _carnivalBlockMembersService.CreateAsync(newBlockMember);
        var result = await _carnivalBlockMembersRepository.GetByIdAsync(newBlockMember.Id);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(0, result.Id);
        Assert.Equal(1, result.CarnivalBlockId);
        Assert.Equal(1, result.MemberId);
        Assert.Equal(RolesEnum.Member, result.Role);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowKeyNotFoundException_WhenMemberDoesNotExist()
    {
        // Arrange
        await _carnivalBlocksRepository.AddAsync(new CarnivalBlockEntity(1, 1, "block", "test", "test", "img"));
        var newBlockMember = new CarnivalBlockMembersEntity(0, 1, 999, RolesEnum.Member);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _carnivalBlockMembersService.CreateAsync(newBlockMember));
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowKeyNotFoundException_WhenCarnivalBlockDoesNotExist()
    {
        // Arrange
        await _membersRepository.AddAsync(new MemberEntity(1, "member", "member@email.com", "123", "img"));
        var newBlockMember = new CarnivalBlockMembersEntity(0, 999, 1, RolesEnum.Member);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _carnivalBlockMembersService.CreateAsync(newBlockMember));
    }
}
