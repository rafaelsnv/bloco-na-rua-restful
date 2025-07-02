using BlocoNaRua.Data.Context;
using BlocoNaRua.Data.Repositories;
using BlocoNaRua.Data.Repositories.Interfaces;

namespace BlocoNaRua.Tests.Data;

public class UsersRepositoryTests
{
    private readonly AppDbContext _contextMock;
    private readonly IUsersRepository _userRepository;
    public UsersRepositoryTests()
    {
        _contextMock = AppDbContextMock.GetContext();
        _userRepository = new UsersRepository(_contextMock);
    }

    private async Task AddData()
    {
        await _userRepository.AddAsync
        (new
            (
                id: 1,
                name: "Test User1",
                email: "test1@test.com",
                password: "password123",
                phone: "1234567890",
                profileImage: "profile1.jpg",
                createdAt: DateTime.UtcNow,
                updatedAt: DateTime.MinValue
            )
        );
        await _userRepository.AddAsync
        (new
            (
                id: 2,
                name: "Test User2",
                email: "test2@test.com",
                password: "password123",
                phone: "1234567890",
                profileImage: "profile2.jpg",
                createdAt: DateTime.UtcNow,
                updatedAt: DateTime.MinValue
            )
        );
    }

    [Fact]
    public async Task GetByIdAsyncExists()
    {
        await AddData();
        var result = await _userRepository.GetByIdAsync(1);

        Assert.Equal(1, result.Id);
    }
}
