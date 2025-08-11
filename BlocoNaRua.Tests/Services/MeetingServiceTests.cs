using BlocoNaRua.Data.Context;
using BlocoNaRua.Data.Repositories;
using BlocoNaRua.Data.Repositories.Interfaces;
using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Domain.Enums;
using BlocoNaRua.Services.Implementations;
using BlocoNaRua.Services.Interfaces;
using BlocoNaRua.Tests.Helpers;

namespace BlocoNaRua.Tests.Services;

public class MeetingServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly IMeetingsRepository _meetingsRepository;
    private readonly IMembersRepository _membersRepository;
    private readonly ICarnivalBlocksRepository _carnivalBlocksRepository;
    private readonly ICarnivalBlockMembersRepository _carnivalBlockMembersRepository;
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;
    private readonly MeetingService _meetingService;

    public MeetingServiceTests()
    {
        var dbName = Guid.NewGuid().ToString();
        _context = TestDbContextFactory.GetContext(dbName);
        _meetingsRepository = new MeetingsRepository(_context);
        _membersRepository = new MembersRepository(_context);
        _carnivalBlocksRepository = new CarnivalBlocksRepository(_context);
        _carnivalBlockMembersRepository = new CarnivalBlockMembersRepository(_context);
        _authorizationServiceMock = new Mock<IAuthorizationService>();
        _meetingService = new MeetingService
        (
            _meetingsRepository,
            _carnivalBlocksRepository,
            _carnivalBlockMembersRepository,
            _authorizationServiceMock.Object
        );
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task AddData(int blockId, int ownerId, int memberId, RolesEnum role)
    {
        if (await _membersRepository.GetByIdAsync(ownerId) is null)
            await _membersRepository.AddAsync(new MemberEntity(ownerId, "Owner", "owner@test.com", "1", "img"));

        if (ownerId != memberId && await _membersRepository.GetByIdAsync(memberId) is null)
            await _membersRepository.AddAsync(new MemberEntity(memberId, "Member", "member@test.com", "2", "img"));

        await _carnivalBlocksRepository.AddAsync(new CarnivalBlockEntity(blockId, ownerId, "Block", "code", "mcode", "img"));

        if (ownerId != memberId)
            await _carnivalBlockMembersRepository.AddAsync(new CarnivalBlockMembersEntity(0, blockId, memberId, role));

        await _meetingsRepository.AddAsync(new MeetingEntity(0, "Meeting", "Desc", "Location", "mcode", DateTime.Now, blockId));
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllMeetings()
    {
        // Arrange
        await AddData(1, 101, 101, RolesEnum.Owner);
        await AddData(2, 201, 201, RolesEnum.Owner);

        // Act
        var result = await _meetingService.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnMeeting_WhenMeetingExists()
    {
        // Arrange
        await AddData(1, 101, 101, RolesEnum.Owner);

        // Act
        var result = await _meetingService.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateMeeting_WhenMemberIsOwner()
    {
        // Arrange
        await AddData(1, 101, 101, RolesEnum.Owner);
        var newMeeting = new MeetingEntity(0, "New Meeting", "Desc", "Location", "", DateTime.Now, 1);

        // Act
        _authorizationServiceMock.Setup(s => s.GetMemberRole(1, 101)).ReturnsAsync(RolesEnum.Owner);
        var result = await _meetingService.CreateAsync(newMeeting, 101);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(0, result.Id);
        Assert.False(string.IsNullOrEmpty(result.MeetingCode));
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateMeeting_WhenMemberIsManager()
    {
        // Arrange
        await AddData(1, 101, 102, RolesEnum.Manager);
        var updatedModel = new MeetingEntity(1, "Updated Meeting", "New Desc", "New Location", "", DateTime.Now, 1);

        // Act
        _authorizationServiceMock.Setup(s => s.GetMemberRole(1, 102)).ReturnsAsync(RolesEnum.Manager);
        var result = await _meetingService.UpdateAsync(1, updatedModel, 102);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Meeting", result.Name);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteMeeting_WhenMemberIsOwner()
    {
        // Arrange
        await AddData(1, 101, 101, RolesEnum.Owner);

        // Act
        _authorizationServiceMock.Setup(s => s.GetMemberRole(1, 101)).ReturnsAsync(RolesEnum.Owner);
        var result = await _meetingService.DeleteAsync(1, 101);

        // Assert
        Assert.True(result);
        var deletedMeeting = await _meetingsRepository.GetByIdAsync(1);
        Assert.Null(deletedMeeting);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowUnauthorizedAccessException_WhenMemberIsRegularMember()
    {
        // Arrange
        await AddData(1, 101, 102, RolesEnum.Member);
        var newMeeting = new MeetingEntity(0, "New Meeting", "Desc", "Location", "", DateTime.Now, 1);

        // Act & Assert
        _authorizationServiceMock.Setup(s => s.GetMemberRole(1, 102)).ReturnsAsync(RolesEnum.Member);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _meetingService.CreateAsync(newMeeting, 102));
    }
}
