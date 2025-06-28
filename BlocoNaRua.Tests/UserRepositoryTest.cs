using BlocoNaRua.Data.Context;
using BlocoNaRua.Data.Repositories;
using BlocoNaRua.Data.Repositories.Interfaces;
using BlocoNaRua.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlocoNaRua.Tests;

public class UsersRepositoryTests : RepositoryBaseTest
{
    private readonly Mock<DbSet<UserEntity>> _dbSetMock;
    private readonly IUsersRepository _userRepository;
    public UsersRepositoryTests()
    {
        _dbSetMock = CreateMockDbSet(new List<UserEntity>
        {
            new
            (
                id: 1,
                name: "Test User1",
                email: "test1@test.com",
                password: "password123",
                phone: "1234567890",
                profileImage: "profile1.jpg",
                createdAt: DateTime.UtcNow,
                updatedAt: DateTime.MinValue
            ),
            new
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
        });

        var contextMock = new Mock<AppDbContext>(new DbContextOptions<AppDbContext>());
        contextMock.Setup(c => c.Set<UserEntity>()).Returns(_dbSetMock.Object);

        _userRepository = new UsersRepository(contextMock.Object);
    }

    [Fact]
    public async Task GetByIdAsyncExists()
    {
        var result = await _userRepository.GetByIdAsync(1);

        Assert.Equal(1, result.Id);
    }
}
