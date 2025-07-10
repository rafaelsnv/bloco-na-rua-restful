using BlocoNaRua.Data.Context;
using BlocoNaRua.Data.Repositories;
using BlocoNaRua.Data.Repositories.Interfaces;

namespace BlocoNaRua.Tests.Data;

public class MembersRepositoryTests
{
    private readonly AppDbContext _contextMock;
    private readonly IMembersRepository _membersRepository;
    public MembersRepositoryTests()
    {
        _contextMock = AppDbContextMock.GetContext();
        _membersRepository = new MembersRepository(_contextMock);
    }

    private async Task AddData()
    {
        await _membersRepository.AddAsync
        (new
            (
                id: 1,
                name: "Test Member1",
                email: "test1@test.com",
                password: "password123",
                phone: "1234567890",
                profileImage: "profile1.jpg"
            )
        );
        await _membersRepository.AddAsync
        (new
            (
                id: 2,
                name: "Test Member2",
                email: "test2@test.com",
                password: "password123",
                phone: "1234567890",
                profileImage: "profile2.jpg"
            )
        );
    }

    [Fact]
    public async Task GetByIdAsyncExists()
    {
        await AddData();
        var result = await _membersRepository.GetByIdAsync(1);

        Assert.Equal(1, result.Id);
    }
}
