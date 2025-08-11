using BlocoNaRua.Data.Context;
using BlocoNaRua.Data.Repositories;
using BlocoNaRua.Data.Repositories.Interfaces;
using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Domain.Enums;
using BlocoNaRua.Services.Implementations;
using BlocoNaRua.Services.Interfaces;
using BlocoNaRua.Tests.Helpers;

namespace BlocoNaRua.Tests.Services;

public class MeetingPresenceServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly IMeetingPresencesRepository _meetingPresencesRepository;
    private readonly IMembersRepository _membersRepository;
    private readonly ICarnivalBlocksRepository _carnivalBlocksRepository;
    private readonly ICarnivalBlockMembersRepository _carnivalBlockMembersRepository;
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;
    private readonly MeetingPresenceService _meetingPresenceService;

    public MeetingPresenceServiceTests()
    {
        var dbName = Guid.NewGuid().ToString();
        _context = TestDbContextFactory.GetContext(dbName);
        _meetingPresencesRepository = new MeetingPresencesRepository(_context);
        _membersRepository = new MembersRepository(_context);
        _carnivalBlocksRepository = new CarnivalBlocksRepository(_context);
        _carnivalBlockMembersRepository = new CarnivalBlockMembersRepository(_context);
        _authorizationServiceMock = new Mock<IAuthorizationService>();
        _meetingPresenceService = new MeetingPresenceService
        (
            _meetingPresencesRepository,
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

    private async Task AddData(int blockId, int ownerId, int memberId, RolesEnum role, int meetingId)
    {
        if (await _membersRepository.GetByIdAsync(ownerId) is null)
            await _membersRepository.AddAsync(new MemberEntity(ownerId, "Owner", "owner@test.com", "1", "img"));

        if (ownerId != memberId && await _membersRepository.GetByIdAsync(memberId) is null)
            await _membersRepository.AddAsync(new MemberEntity(memberId, "Member", "member@test.com", "2", "img"));

        await _carnivalBlocksRepository.AddAsync(new CarnivalBlockEntity(blockId, ownerId, "Block", "code", "mcode", "img"));

        if (ownerId != memberId)
            await _carnivalBlockMembersRepository.AddAsync(new CarnivalBlockMembersEntity(0, blockId, memberId, role));

        await _context.Meetings.AddAsync(new MeetingEntity(meetingId, "Meeting", "Desc", "Loc", "code", DateTime.Now, blockId));
        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllMeetingPresences()
    {
        // Arrange
        await AddData(1, 101, 101, RolesEnum.Owner, 1);
        await _meetingPresencesRepository.AddAsync(new MeetingPresenceEntity(0) { MemberId = 101, MeetingId = 1, CarnivalBlockId = 1, IsPresent = true });

        // Act
        var result = await _meetingPresenceService.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnMeetingPresence_WhenMeetingPresenceExists()
    {
        // Arrange
        await AddData(1, 101, 101, RolesEnum.Owner, 1);
        var presence = await _meetingPresencesRepository.AddAsync(new MeetingPresenceEntity(0) { MemberId = 101, MeetingId = 1, CarnivalBlockId = 1, IsPresent = true });

        // Act
        var result = await _meetingPresenceService.GetByIdAsync(presence.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(presence.Id, result.Id);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateMeetingPresence_WhenLoggedMemberIsSelf()
    {
        // Arrange
        await AddData(1, 101, 101, RolesEnum.Owner, 1);
        var newPresence = new MeetingPresenceEntity(0) { MemberId = 101, MeetingId = 1, CarnivalBlockId = 1, IsPresent = true };

        // Act
        var result = await _meetingPresenceService.CreateAsync(newPresence, 101);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(0, result.Id);
        Assert.True(result.IsPresent);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateMeetingPresence_WhenLoggedMemberIsOwner()
    {
        // Arrange
        await AddData(1, 101, 102, RolesEnum.Member, 1);
        var newPresence = new MeetingPresenceEntity(0) { MemberId = 102, MeetingId = 1, CarnivalBlockId = 1, IsPresent = true };

        // Act
        _authorizationServiceMock.Setup(s => s.GetMemberRole(1, 101)).ReturnsAsync(RolesEnum.Owner);
        var result = await _meetingPresenceService.CreateAsync(newPresence, 101);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(0, result.Id);
        Assert.True(result.IsPresent);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowUnauthorizedAccessException_WhenLoggedMemberIsNotAuthorized()
    {
        // Arrange
        await AddData(1, 101, 102, RolesEnum.Member, 1);
        var newPresence = new MeetingPresenceEntity(0) { MemberId = 102, MeetingId = 1, CarnivalBlockId = 1, IsPresent = true };

        // Act & Assert
        _authorizationServiceMock.Setup(s => s.GetMemberRole(1, 103)).ReturnsAsync(RolesEnum.Member);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _meetingPresenceService.CreateAsync(newPresence, 103));
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateMeetingPresence_WhenLoggedMemberIsSelf()
    {
        // Arrange
        await AddData(1, 101, 101, RolesEnum.Owner, 1);
        var presence = await _meetingPresencesRepository.AddAsync(new MeetingPresenceEntity(0) { MemberId = 101, MeetingId = 1, CarnivalBlockId = 1, IsPresent = false });
        var updatedModel = new MeetingPresenceEntity(presence.Id) { MemberId = 101, MeetingId = 1, CarnivalBlockId = 1, IsPresent = true };

        // Act
        var result = await _meetingPresenceService.UpdateAsync(presence.Id, updatedModel, 101);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsPresent);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateMeetingPresence_WhenLoggedMemberIsOwner()
    {
        // Arrange
        await AddData(1, 101, 102, RolesEnum.Member, 1);
        var presence = await _meetingPresencesRepository.AddAsync(new MeetingPresenceEntity(0) { MemberId = 102, MeetingId = 1, CarnivalBlockId = 1, IsPresent = false });
        var updatedModel = new MeetingPresenceEntity(presence.Id) { MemberId = 102, MeetingId = 1, CarnivalBlockId = 1, IsPresent = true };

        // Act
        _authorizationServiceMock.Setup(s => s.GetMemberRole(1, 101)).ReturnsAsync(RolesEnum.Owner);
        var result = await _meetingPresenceService.UpdateAsync(presence.Id, updatedModel, 101);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsPresent);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowUnauthorizedAccessException_WhenLoggedMemberIsNotAuthorized()
    {
        // Arrange
        await AddData(1, 101, 102, RolesEnum.Member, 1);
        var presence = await _meetingPresencesRepository.AddAsync(new MeetingPresenceEntity(0) { MemberId = 102, MeetingId = 1, CarnivalBlockId = 1, IsPresent = false });
        var updatedModel = new MeetingPresenceEntity(presence.Id) { MemberId = 102, MeetingId = 1, CarnivalBlockId = 1, IsPresent = true };

        // Act & Assert
        _authorizationServiceMock.Setup(s => s.GetMemberRole(1, 103)).ReturnsAsync(RolesEnum.Member);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _meetingPresenceService.UpdateAsync(presence.Id, updatedModel, 103));
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteMeetingPresence_WhenLoggedMemberIsSelf()
    {
        // Arrange
        await AddData(1, 101, 101, RolesEnum.Owner, 1);
        var presence = await _meetingPresencesRepository.AddAsync(new MeetingPresenceEntity(0) { MemberId = 101, MeetingId = 1, CarnivalBlockId = 1, IsPresent = true });

        // Act
        var result = await _meetingPresenceService.DeleteAsync(presence.Id, 101);

        // Assert
        Assert.True(result);
        var deletedPresence = await _meetingPresencesRepository.GetByIdAsync(presence.Id);
        Assert.Null(deletedPresence);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteMeetingPresence_WhenLoggedMemberIsOwner()
    {
        // Arrange
        await AddData(1, 101, 102, RolesEnum.Member, 1);
        var presence = await _meetingPresencesRepository.AddAsync(new MeetingPresenceEntity(0) { MemberId = 102, MeetingId = 1, CarnivalBlockId = 1, IsPresent = true });

        // Act
        _authorizationServiceMock.Setup(s => s.GetMemberRole(1, 101)).ReturnsAsync(RolesEnum.Owner);
        var result = await _meetingPresenceService.DeleteAsync(presence.Id, 101);

        // Assert
        Assert.True(result);
        var deletedPresence = await _meetingPresencesRepository.GetByIdAsync(presence.Id);
        Assert.Null(deletedPresence);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowUnauthorizedAccessException_WhenLoggedMemberIsNotAuthorized()
    {
        // Arrange
        await AddData(1, 101, 102, RolesEnum.Member, 1);
        var presence = await _meetingPresencesRepository.AddAsync(new MeetingPresenceEntity(0) { MemberId = 102, MeetingId = 1, CarnivalBlockId = 1, IsPresent = true });

        // Act & Assert
        _authorizationServiceMock.Setup(s => s.GetMemberRole(1, 103)).ReturnsAsync(RolesEnum.Member);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _meetingPresenceService.DeleteAsync(presence.Id, 103));
    }
}
