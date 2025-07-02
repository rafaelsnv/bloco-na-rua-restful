using BlocoNaRua.Data.Context;
using BlocoNaRua.Data.Repositories;
using BlocoNaRua.Data.Repositories.Interfaces;

namespace BlocoNaRua.Tests.Data;

public class CarnivalBlocksRepositoryTests
{
    private readonly AppDbContext _contextMock;
    private readonly ICarnivalBlocksRepository _carnivalBlocksRepository;
    public CarnivalBlocksRepositoryTests()
    {
        _contextMock = AppDbContextMock.GetContext();
        _carnivalBlocksRepository = new CarnivalBlocksRepository(_contextMock);
    }

    private async Task AddData()
    {
        await _carnivalBlocksRepository.AddAsync
        (new
            (
                id: 1,
                name: "Test Carnival Block1",
                owner: "User1",
                inviteCode: "invite_code",
                managersInviteCode: "managers_invite_code",
                carnivalBlockImage: "block_logo.jpg",
                createdAt: DateTime.UtcNow,
                updatedAt: DateTime.MinValue
            )

        );
    }

    [Fact]
    public async Task UpdateAsync()
    {
        await AddData();

        var act = await _carnivalBlocksRepository.GetByIdAsync(1);
        act.Name = "Updated Carnival Block";
        act.Owner = "Updated User";
        act.InviteCode = "updated_invite_code";
        act.ManagersInviteCode = "updated_managers_invite_code";
        act.CarnivalBlockImage = "updated_block_logo.jpg";
        act.UpdatedAt = DateTime.UtcNow;
        await _carnivalBlocksRepository.UpdateAsync(act);

        var result = await _carnivalBlocksRepository.GetByIdAsync(1);
        Assert.Equal(act, result);
    }
}
