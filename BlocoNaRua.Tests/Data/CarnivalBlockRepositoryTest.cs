using BlocoNaRua.Data.Context;
using BlocoNaRua.Data.Repositories;
using BlocoNaRua.Data.Repositories.Interfaces;
using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Domain.Enums;

namespace BlocoNaRua.Tests.Data;

public class CarnivalBlocksRepositoryTests
{
    private readonly AppDbContext _contextMock;
    private readonly Mock<IMembersRepository> _membersRepositoryMock;
    private readonly ICarnivalBlocksRepository _carnivalBlocksRepository;
    private readonly Mock<ICarnivalBlockMembersRepository> _carnivalBlockMembersRepoMock;
    public CarnivalBlocksRepositoryTests()
    {
        _contextMock = AppDbContextMock.GetContext();

        _membersRepositoryMock = new Mock<IMembersRepository>();
        _carnivalBlockMembersRepoMock = new Mock<ICarnivalBlockMembersRepository>();

        _carnivalBlocksRepository = new CarnivalBlocksRepository
        (
            _contextMock,
            _membersRepositoryMock.Object,
            _carnivalBlockMembersRepoMock.Object
        );
    }

    private async Task AddData(int id)
    {
        _membersRepositoryMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(new MemberEntity(
                id: id,
                name: "Test Member",
                email: "test@test.com",
                password: "password123",
                phone: "1234567890",
                profileImage: "profile_image.jpg"
            ));

        await _carnivalBlocksRepository.AddAsync
        (new
            (
                id: id,
                name: "Test Carnival Block1",
                ownerId: id,
                inviteCode: "invite_code",
                managersInviteCode: "managers_invite_code",
                carnivalBlockImage: "block_logo.jpg"
            )

        );
    }

    [Fact]
    public async Task UpdateAsync_IsOwner()
    {
        await AddData(1);

        var act = await _carnivalBlocksRepository.GetByIdAsync(1);
        act.Name = "Updated Carnival Block";
        act.OwnerId = 1;
        act.InviteCode = "updated_invite_code";
        act.ManagersInviteCode = "updated_managers_invite_code";
        act.CarnivalBlockImage = "updated_block_logo.jpg";
        act.UpdatedAt = DateTime.UtcNow;
        await _carnivalBlocksRepository.UpdateAsync(1, act);

        var result = await _carnivalBlocksRepository.GetByIdAsync(1);
        Assert.Equal(act, result);
    }

    [Fact]
    public async Task UpdateAsync_CarnivalBlock_KeyNotFoundEx()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
        {
            await _carnivalBlocksRepository.UpdateAsync(1, new CarnivalBlockEntity(
                id: 999,
                name: "Doesn't matter",
                ownerId: 1,
                inviteCode: "",
                managersInviteCode: "",
                carnivalBlockImage: ""
            ));
        });
    }

    [Fact]
    public async Task UpdateAsync_Member_KeyNotFoundEx()
    {

        await AddData(2);

        var act = await _carnivalBlocksRepository.GetByIdAsync(1);

        await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
        {
            await _carnivalBlocksRepository.UpdateAsync(2, act);
        });
    }

    [Fact]
    public async Task UpdateAsync_Member_UnauthorizedEx()
    {
        await AddData(3);

        _carnivalBlockMembersRepoMock
            .Setup(r => r.GetByIdAsync(2))
            .ReturnsAsync(new CarnivalBlockMembersEntity(
                id: 2,
                carnivalBlockId: 3,
                memberId: 2,
                role: RolesEnum.Member
            ));

        var act = await _carnivalBlocksRepository.GetByIdAsync(3);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
        {
            await _carnivalBlocksRepository.UpdateAsync(2, act);
        });
    }

    [Fact]
    public async Task UpdateAsync_IsManager()
    {
        await AddData(4);

        _carnivalBlockMembersRepoMock
            .Setup(r => r.GetByIdAsync(2))
            .ReturnsAsync(new CarnivalBlockMembersEntity(
                id: 2,
                carnivalBlockId: 4,
                memberId: 2,
                role: RolesEnum.Manager
            ));

        var act = await _carnivalBlocksRepository.GetByIdAsync(4);

        var result = await _carnivalBlocksRepository.UpdateAsync(2, act);
        Assert.True(result);
    }

}
