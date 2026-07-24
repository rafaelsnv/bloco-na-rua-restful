using BlocoNaRua.Data.Context;
using BlocoNaRua.Data.Repositories;
using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Domain.Enums;
using BlocoNaRua.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace BlocoNaRua.Tests.Data;

public class MembersRepositoryTests
{
    private AppDbContext CreateContext() => TestDbContextFactory.GetContext($"MembersRepo_{Guid.NewGuid()}");

    [Fact]
    public async Task GetByUuidAsync_ReturnsMember_WhenExists()
    {
        // Arrange
        await using var context = CreateContext();
        var uuid = Guid.NewGuid();
        var member = new MemberEntity(1, "Test", "test@test.com", "123", "img.jpg", uuid);
        context.Members.Add(member);
        await context.SaveChangesAsync();

        var repo = new MembersRepository(context);

        // Act
        var result = await repo.GetByUuidAsync(uuid);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test", result.Name);
        Assert.Equal(uuid, result.Uuid);
    }

    [Fact]
    public async Task GetByUuidAsync_ReturnsNull_WhenNotExists()
    {
        // Arrange
        await using var context = CreateContext();
        var repo = new MembersRepository(context);

        // Act
        var result = await repo.GetByUuidAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }
}

public class MeetingsRepositoryTests
{
    private AppDbContext CreateContext() => TestDbContextFactory.GetContext($"MeetingsRepo_{Guid.NewGuid()}");

    [Fact]
    public async Task GetAllByBlockIdAsync_ReturnsMeetings_WhenBlockHasMeetings()
    {
        // Arrange
        await using var context = CreateContext();
        var block = new CarnivalBlockEntity(1, 10, "Block", "CODE", "MGR", "img.jpg");
        context.CarnivalBlocks.Add(block);
        var m1 = new MeetingEntity(1, "Meeting 1", "Desc", "Loc", "M1", DateTime.Now, 1);
        var m2 = new MeetingEntity(2, "Meeting 2", "Desc", "Loc", "M2", DateTime.Now, 1);
        context.Meetings.AddRange(m1, m2);
        await context.SaveChangesAsync();

        var repo = new MeetingsRepository(context);

        // Act
        var result = await repo.GetAllByBlockIdAsync(1);

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetAllByBlockIdAsync_ReturnsEmpty_WhenBlockHasNoMeetings()
    {
        // Arrange
        await using var context = CreateContext();
        var block = new CarnivalBlockEntity(1, 10, "Block", "CODE", "MGR", "img.jpg");
        context.CarnivalBlocks.Add(block);
        await context.SaveChangesAsync();

        var repo = new MeetingsRepository(context);

        // Act
        var result = await repo.GetAllByBlockIdAsync(1);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByBlockIdsAsync_ReturnsMeetings_WhenBlocksHaveMeetings()
    {
        // Arrange
        await using var context = CreateContext();
        var block1 = new CarnivalBlockEntity(1, 10, "Block1", "CODE1", "MGR1", "img1.jpg");
        var block2 = new CarnivalBlockEntity(2, 11, "Block2", "CODE2", "MGR2", "img2.jpg");
        context.CarnivalBlocks.AddRange(block1, block2);
        var m1 = new MeetingEntity(1, "Meeting 1", "Desc", "Loc", "M1", DateTime.Now, 1);
        var m2 = new MeetingEntity(2, "Meeting 2", "Desc", "Loc", "M2", DateTime.Now, 2);
        var m3 = new MeetingEntity(3, "Meeting 3", "Desc", "Loc", "M3", DateTime.Now, 1);
        context.Meetings.AddRange(m1, m2, m3);
        await context.SaveChangesAsync();

        var repo = new MeetingsRepository(context);

        // Act
        var result = await repo.GetByBlockIdsAsync(new List<int> { 1, 2 });

        // Assert
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task GetByBlockIdsAsync_ReturnsEmpty_WhenNoBlocksHaveMeetings()
    {
        // Arrange
        await using var context = CreateContext();
        var repo = new MeetingsRepository(context);

        // Act
        var result = await repo.GetByBlockIdsAsync(new List<int> { 1, 2 });

        // Assert
        Assert.Empty(result);
    }
}

public class CarnivalBlockMembersRepositoryTests
{
    private AppDbContext CreateContext() => TestDbContextFactory.GetContext($"CarnivalBlockMembersRepo_{Guid.NewGuid()}");

    [Fact]
    public async Task GetMemberRole_ReturnsRole_WhenMemberInBlock()
    {
        // Arrange
        await using var context = CreateContext();
        var member = new MemberEntity(1, "Member", "m@m.com", "123", "img.jpg", Guid.NewGuid());
        var block = new CarnivalBlockEntity(1, 10, "Block", "CODE", "MGR", "img.jpg");
        context.Members.Add(member);
        context.CarnivalBlocks.Add(block);
        var cbMember = new CarnivalBlockMembersEntity(1, 1, 1, RolesEnum.Manager);
        context.CarnivalBlockMembers.Add(cbMember);
        await context.SaveChangesAsync();

        var repo = new CarnivalBlockMembersRepository(context);

        // Act
        var result = await repo.GetMemberRole(1, 1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(RolesEnum.Manager, result);
    }

    [Fact]
    public async Task GetMemberRole_ReturnsNull_WhenMemberNotInBlock()
    {
        // Arrange
        await using var context = CreateContext();
        var member = new MemberEntity(1, "Member", "m@m.com", "123", "img.jpg", Guid.NewGuid());
        var block = new CarnivalBlockEntity(1, 10, "Block", "CODE", "MGR", "img.jpg");
        context.Members.Add(member);
        context.CarnivalBlocks.Add(block);
        await context.SaveChangesAsync();

        var repo = new CarnivalBlockMembersRepository(context);

        // Act
        var result = await repo.GetMemberRole(1, 1);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByBlockIdAsync_ReturnsMembers_WhenBlockHasMembers()
    {
        // Arrange
        await using var context = CreateContext();
        var member1 = new MemberEntity(1, "M1", "m1@m.com", "123", "img.jpg", Guid.NewGuid());
        var member2 = new MemberEntity(2, "M2", "m2@m.com", "456", "img.jpg", Guid.NewGuid());
        var block = new CarnivalBlockEntity(1, 10, "Block", "CODE", "MGR", "img.jpg");
        context.Members.AddRange(member1, member2);
        context.CarnivalBlocks.Add(block);
        context.CarnivalBlockMembers.Add(new CarnivalBlockMembersEntity(1, 1, 1, RolesEnum.Member));
        context.CarnivalBlockMembers.Add(new CarnivalBlockMembersEntity(2, 1, 2, RolesEnum.Manager));
        await context.SaveChangesAsync();

        var repo = new CarnivalBlockMembersRepository(context);

        // Act
        var result = await repo.GetByBlockIdAsync(1);

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetByBlockIdAsync_ReturnsEmpty_WhenBlockHasNoMembers()
    {
        // Arrange
        await using var context = CreateContext();
        var block = new CarnivalBlockEntity(1, 10, "Block", "CODE", "MGR", "img.jpg");
        context.CarnivalBlocks.Add(block);
        await context.SaveChangesAsync();

        var repo = new CarnivalBlockMembersRepository(context);

        // Act
        var result = await repo.GetByBlockIdAsync(1);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByMemberIdAsync_ReturnsMemberships_WithCarnivalBlockIncluded()
    {
        // Arrange
        await using var context = CreateContext();
        var member = new MemberEntity(1, "Member", "m@m.com", "123", "img.jpg", Guid.NewGuid());
        var block = new CarnivalBlockEntity(1, 10, "Block", "CODE", "MGR", "img.jpg");
        context.Members.Add(member);
        context.CarnivalBlocks.Add(block);
        context.CarnivalBlockMembers.Add(new CarnivalBlockMembersEntity(1, 1, 1, RolesEnum.Member));
        await context.SaveChangesAsync();

        var repo = new CarnivalBlockMembersRepository(context);

        // Act
        var result = await repo.GetByMemberIdAsync(1);

        // Assert
        Assert.Single(result);
        Assert.NotNull(result[0].CarnivalBlock);
        Assert.Equal("Block", result[0].CarnivalBlock.Name);
    }

    [Fact]
    public async Task GetByMemberIdAsync_ReturnsEmpty_WhenMemberHasNoMemberships()
    {
        // Arrange
        await using var context = CreateContext();
        var member = new MemberEntity(1, "Member", "m@m.com", "123", "img.jpg", Guid.NewGuid());
        context.Members.Add(member);
        await context.SaveChangesAsync();

        var repo = new CarnivalBlockMembersRepository(context);

        // Act
        var result = await repo.GetByMemberIdAsync(1);

        // Assert
        Assert.Empty(result);
    }
}
