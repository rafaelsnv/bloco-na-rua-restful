using BlocoNaRua.Data.Context;
using BlocoNaRua.Data.Repositories;
using BlocoNaRua.Data.Repositories.Interfaces;
using BlocoNaRua.Domain.Entities;

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

    private async Task AddData()
    {

        await _carnivalBlocksRepository.AddAsync
        (new
            (
                id: 1,
                name: "Test Carnival Block1",
                ownerId: 1,
                inviteCode: "invite_code",
                managersInviteCode: "managers_invite_code",
                carnivalBlockImage: "block_logo.jpg"
            )

        );
    }

    [Fact]
    public async Task UpdateAsync()
    {
        _membersRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new MemberEntity(
                id: 1,
                name: "Test Member",
                email: "test@test.com",
                password: "password123",
                phone: "1234567890",
                profileImage: "profile_image.jpg"
            ));

        await AddData();

        var act = await _carnivalBlocksRepository.GetByIdAsync(1);
        act.Name = "Updated Carnival Block";
        act.OwnerId = 1;
        act.InviteCode = "updated_invite_code";
        act.ManagersInviteCode = "updated_managers_invite_code";
        act.CarnivalBlockImage = "updated_block_logo.jpg";
        act.UpdatedAt = DateTime.UtcNow;
        await _carnivalBlocksRepository.UpdateAsync(act);

        var result = await _carnivalBlocksRepository.GetByIdAsync(1);
        Assert.Equal(act, result);
    }
}
