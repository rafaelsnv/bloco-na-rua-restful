using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Domain.Enums;
using BlocoNaRua.Restful.Controllers;
using BlocoNaRua.Restful.Models.CarnivalBlockMember;
using BlocoNaRua.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BlocoNaRua.Tests.Restful;

public class CarnivalBlockMembersControllerTests
{
    private readonly Mock<ILogger<CarnivalBlockMembersController>> _loggerMock = new();
    private readonly Mock<ICarnivalBlockMembersService> _serviceMock = new();

    private CarnivalBlockMembersController CreateController() => new(_loggerMock.Object, _serviceMock.Object);

    [Fact]
    public async Task GetAllBlocksMembers_ReturnsOkWithList()
    {
        // Arrange
        var entities = new List<CarnivalBlockMembersEntity>
        {
            new(1, 1, 1, RolesEnum.Owner),
            new(2, 1, 2, RolesEnum.Manager)
        };
        _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(entities);

        var controller = CreateController();

        // Act
        var result = await controller.GetAllBlocksMembers();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsAssignableFrom<List<CarnivalBlockMembersEntity>>(okResult.Value);
    }

    [Fact]
    public async Task GetBlocksMembersById_ReturnsOkWithEntity()
    {
        // Arrange
        var entity = new CarnivalBlockMembersEntity(1, 1, 1, RolesEnum.Owner);
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(entity);

        var controller = CreateController();

        // Act
        var result = await controller.GetBlocksMembersById(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<CarnivalBlockMembersEntity>(okResult.Value);
    }

    [Fact]
    public async Task GetBlocksMembersById_ReturnsNotFound()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync((CarnivalBlockMembersEntity?)null);

        var controller = CreateController();

        // Act
        var result = await controller.GetBlocksMembersById(1);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task CreateCarnivalBlockMember_ReturnsCreatedAtAction()
    {
        // Arrange
        var createDto = new CarnivalBlockMemberCreate(
            CarnivalBlockId: 1,
            MemberId: 1,
            Role: RolesEnum.Member
        );

        var controller = CreateController();

        // Act
        var result = await controller.CreateCarnivalBlockMember(createDto);

        // Assert
        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal("GetBlocksMembersById", created.ActionName);
        Assert.IsType<CarnivalBlockMembersEntity>(created.Value);
    }

    [Fact]
    public async Task CreateCarnivalBlockMember_ReturnsBadRequest_WhenDtoIsNull()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.CreateCarnivalBlockMember(null);

        // Assert
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task CreateCarnivalBlockMember_ReturnsNotFound_WhenMemberDoesNotExist()
    {
        // Arrange
        var createDto = new CarnivalBlockMemberCreate(
            CarnivalBlockId: 1,
            MemberId: 999,
            Role: RolesEnum.Member
        );

        _serviceMock.Setup(s => s.CreateAsync(It.IsAny<CarnivalBlockMembersEntity>()))
            .ThrowsAsync(new KeyNotFoundException("Member does not exist."));

        var controller = CreateController();

        // Act
        var result = await controller.CreateCarnivalBlockMember(createDto);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Member does not exist.", notFoundResult.Value);
    }

    [Fact]
    public async Task CreateCarnivalBlockMember_ReturnsNotFound_WhenCarnivalBlockDoesNotExist()
    {
        // Arrange
        var createDto = new CarnivalBlockMemberCreate(
            CarnivalBlockId: 999,
            MemberId: 1,
            Role: RolesEnum.Member
        );

        _serviceMock.Setup(s => s.CreateAsync(It.IsAny<CarnivalBlockMembersEntity>()))
            .ThrowsAsync(new KeyNotFoundException("Carnival block does not exist."));

        var controller = CreateController();

        // Act
        var result = await controller.CreateCarnivalBlockMember(createDto);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Carnival block does not exist.", notFoundResult.Value);
    }

    [Fact]
    public async Task CreateCarnivalBlockMember_ReturnsBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        var createDto = new CarnivalBlockMemberCreate(
            CarnivalBlockId: 1,
            MemberId: 1,
            Role: RolesEnum.Member
        );

        _serviceMock.Setup(s => s.CreateAsync(It.IsAny<CarnivalBlockMembersEntity>()))
            .ThrowsAsync(new InvalidOperationException("Something went wrong"));

        var controller = CreateController();

        // Act
        var result = await controller.CreateCarnivalBlockMember(createDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Something went wrong", badRequestResult.Value);
    }
}
