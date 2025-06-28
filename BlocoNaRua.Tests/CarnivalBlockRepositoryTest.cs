using BlocoNaRua.Data.Context;
using BlocoNaRua.Data.Repositories;
using BlocoNaRua.Data.Repositories.Interfaces;
using BlocoNaRua.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlocoNaRua.Tests;

public class CarnivalBlocksRepositoryTests : RepositoryBaseTest
{
    private readonly Mock<DbSet<CarnivalBlockEntity>> _dbSetMock;
    private readonly ICarnivalBlocksRepository _carnivalBlocksRepository;
    public CarnivalBlocksRepositoryTests()
    {
        _dbSetMock = CreateMockDbSet(new List<CarnivalBlockEntity>
        {
            new
            (
                id: 1,
                name: "Test Carnival Block1",
                owner: "User1",
                inviteCode: "invite_code",
                managersInviteCode: "managers_invite_code",
                carnivalBlockImage: "block_logo.jpg",
                createdAt: DateTime.UtcNow,
                updatedAt: DateTime.MinValue
            ),
            new
            (
                id: 2,
                name: "Test Carnival Block2",
                owner: "User2",
                inviteCode: "invite_code2",
                managersInviteCode: "managers_invite_code2",
                carnivalBlockImage: "block_logo2.jpg",
                createdAt: DateTime.UtcNow,
                updatedAt: DateTime.MinValue
            )
        });

        var contextMock = new Mock<AppDbContext>(new DbContextOptions<AppDbContext>());
        contextMock.Setup(c => c.Set<CarnivalBlockEntity>()).Returns(_dbSetMock.Object);

        _carnivalBlocksRepository = new CarnivalBlocksRepository(contextMock.Object);
    }

    [Fact]
    public async Task UpdateAsync()
    {
        var act = new CarnivalBlockEntity
        (
            id: 1,
            name: "Updated Carnival Block",
            owner: "Updated User",
            inviteCode: "updated_invite_code",
            managersInviteCode: "updated_managers_invite_code",
            carnivalBlockImage: "updated_block_logo.jpg",
            createdAt: DateTime.UtcNow,
            updatedAt: DateTime.UtcNow
        );
        await _carnivalBlocksRepository.UpdateAsync(act);

        var result = await _carnivalBlocksRepository.GetByIdAsync(1);

        Assert.Equal(act, result);
    }
}
