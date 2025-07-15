using BlocoNaRua.Data.Context;
using BlocoNaRua.Data.Repositories;
using BlocoNaRua.Data.Repositories.Interfaces;
using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Domain.Enums;

namespace BlocoNaRua.Tests.Data;

public class CarnivalBlockMembersRepositoryTests
{
    private readonly AppDbContext _contextMock;
    private readonly Mock<IMembersRepository> _membersRepositoryMock;
    private readonly ICarnivalBlockMembersRepository _carnivalBlockMembersRepo;
    private readonly Mock<ICarnivalBlocksRepository> _carnivalBlocksRepoMock;
    public CarnivalBlockMembersRepositoryTests()
    {
        _contextMock = AppDbContextMock.GetContext();

        _membersRepositoryMock = new Mock<IMembersRepository>();
        _carnivalBlocksRepoMock = new Mock<ICarnivalBlocksRepository>();

        _carnivalBlockMembersRepo = new CarnivalBlockMembersRepository
        (
            _contextMock,
            _membersRepositoryMock.Object,
            _carnivalBlocksRepoMock.Object
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

        // await _carnivalBlockMembersRepo.AddAsync
        // (new
        //     (
        //         id: id,
        //         name: "Test Carnival Block1",
        //         ownerId: id,
        //         inviteCode: "invite_code",
        //         managersInviteCode: "managers_invite_code",
        //         carnivalBlockImage: "block_logo.jpg"
        //     )

        // );
    }


}
