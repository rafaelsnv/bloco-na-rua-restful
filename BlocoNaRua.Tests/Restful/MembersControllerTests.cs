using BlocoNaRua.Data.Repositories.Interfaces;
using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Restful.Controllers;
using BlocoNaRua.Restful.Models.Member;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BlocoNaRua.Tests.Restful;

public class MembersControllerTests
{
    private readonly Mock<ILogger<MembersController>> _loggerMock = new();
    private readonly Mock<IMembersRepository> _repoMock = new();

    private MembersController CreateController() =>
        new(_loggerMock.Object, _repoMock.Object);

    [Fact]
    public async Task GetAllMembers_Success()
    {
        // Arrange
        var members = new List<MemberEntity>
        { new
            (
                id: 1,
                name: "Test",
                email: "test@email.com",
                password: "pass",
                phone: "123",
                profileImage: "img"
            )
        };
        _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(members);

        var controller = CreateController();

        // Act
        var result = await controller.GetAllMembers();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(members, okResult.Value);
    }

    [Fact]
    public async Task GetMemberById_Success()
    {
        var member = new MemberEntity
        (
            1,
            "Test",
            "test@email.com",
            "pass",
            "123",
            "img"
        );
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(member);

        var controller = CreateController();

        var result = await controller.GetMemberById(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(member, okResult.Value);
    }

    [Fact]
    public async Task GetMemberById_NotFound()
    {
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((MemberEntity)null);

        var controller = CreateController();

        var result = await controller.GetMemberById(1);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task CreateMember_BadRequest()
    {
        var controller = CreateController();

        var result = await controller.CreateMember(null);

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task CreateMember_Success()
    {
        var memberCreate = new MemberCreate
        (
            Name: "Test",
            Email: "test@email.com",
            Password: "pass",
            Phone: "123",
            ProfileImage: "img"
        );
        var entity = new MemberEntity(1, memberCreate.Name, memberCreate.Email, memberCreate.Password, memberCreate.Phone, memberCreate.ProfileImage);
        _repoMock.Setup(r => r.AddAsync(It.IsAny<MemberEntity>())).ReturnsAsync(entity);

        var controller = CreateController();

        var result = await controller.CreateMember(memberCreate);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal("GetMemberById", created.ActionName);
        Assert.Equal(entity, created.Value);
    }

    [Fact]
    public async Task UpdateMember_BadRequest()
    {
        var controller = CreateController();

        var result1 = await controller.UpdateMember(1, null);
        Assert.IsType<BadRequestResult>(result1);

        var member = new Member(2, "Test", "test@email.com", "pass", "123", "img", DateTime.UtcNow, null);
        var result2 = await controller.UpdateMember(1, member);
        Assert.IsType<BadRequestResult>(result2);
    }

    [Fact]
    public async Task UpdateMember_NotFound()
    {
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((MemberEntity)null);

        var controller = CreateController();
        var member = new Member(1, null, null, null, null, null, null, null);

        var result = await controller.UpdateMember(1, member);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task UpdateMember_Success()
    {
        var existing = new MemberEntity(1, "Test", "test@email.com", "pass", "123", null);
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);

        var controller = CreateController();
        var member = new Member(1, "Updated", null, null, null, null, null, null);

        var result = await controller.UpdateMember(1, member);

        Assert.IsType<AcceptedResult>(result);
        _repoMock.Verify(r => r.UpdateAsync(It.Is<MemberEntity>(m => m.Name == "Updated")), Times.Once);
    }

    [Fact]
    public async Task DeleteMember_NotFound()
    {
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((MemberEntity)null);

        var controller = CreateController();

        var result = await controller.DeleteMember(1);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteMember_Success()
    {
        var member = new MemberEntity(1, "Test", "test@email.com", "pass", "123", "img");
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(member);

        var controller = CreateController();

        var result = await controller.DeleteMember(1);

        Assert.IsType<NoContentResult>(result);
        _repoMock.Verify(r => r.DeleteAsync(member), Times.Once);
    }
}
