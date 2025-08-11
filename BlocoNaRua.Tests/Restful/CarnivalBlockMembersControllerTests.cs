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
        Assert.IsAssignableFrom<List<CarnivalBlockMemberDTO>>(okResult.Value);
    }

    [Fact]
    public async Task GetBlocksMembersByBlockId_ReturnsOkWithListOfEntities()
    {
        // Arrange
        var entities = new List<CarnivalBlockMembersEntity>
        {
            new(1, 1, 1, RolesEnum.Owner),
            new(2, 1, 2, RolesEnum.Manager)
        };
        _serviceMock.Setup(s => s.GetByBlockIdAsync(1)).ReturnsAsync(entities);

        var controller = CreateController();

        // Act
        var result = await controller.GetBlocksMembersByBlockId(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var dtoList = Assert.IsAssignableFrom<List<CarnivalBlockMemberDTO>>(okResult.Value);
        Assert.Equal(2, dtoList.Count);
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
        var result = await controller.CreateCarnivalBlockMember(createDto, 1);

        // Assert
        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal("GetBlocksMembersByBlockId", created.ActionName);
        Assert.IsType<CarnivalBlockMemberDTO>(created.Value);
    }

    [Fact]
    public async Task CreateCarnivalBlockMember_ReturnsBadRequest_WhenDtoIsNull()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.CreateCarnivalBlockMember(null, 1);

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

        _serviceMock.Setup(s => s.CreateAsync(It.IsAny<CarnivalBlockMembersEntity>(), It.IsAny<int>()))
            .ThrowsAsync(new KeyNotFoundException("Member does not exist."));

        var controller = CreateController();

        // Act
        var result = await controller.CreateCarnivalBlockMember(createDto, 1);

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

        _serviceMock.Setup(s => s.CreateAsync(It.IsAny<CarnivalBlockMembersEntity>(), It.IsAny<int>()))
            .ThrowsAsync(new KeyNotFoundException("Carnival block does not exist."));

        var controller = CreateController();

        // Act
        var result = await controller.CreateCarnivalBlockMember(createDto, 1);

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

        _serviceMock.Setup(s => s.CreateAsync(It.IsAny<CarnivalBlockMembersEntity>(), It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("Something went wrong"));

        var controller = CreateController();

        // Act
        var result = await controller.CreateCarnivalBlockMember(createDto, 1);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Something went wrong", badRequestResult.Value);
    }

    [Fact]
    public async Task UpdateCarnivalBlockMember_ReturnsOk_WhenSuccess()
    {
        // Arrange
        var updateDto = new CarnivalBlockMemberUpdate(
            1,
            2,
            RolesEnum.Manager
        );

        var updatedEntity = new CarnivalBlockMembersEntity(1, 1, 2, RolesEnum.Manager);
        _serviceMock.Setup(s => s.UpdateAsync(1, 1, RolesEnum.Manager)).ReturnsAsync(updatedEntity);

        var controller = CreateController();

        // Act
        var result = await controller.UpdateCarnivalBlockMember(1, updateDto, 1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<CarnivalBlockMemberDTO>(okResult.Value);
    }

    [Fact]
    public async Task UpdateCarnivalBlockMember_ReturnsBadRequest_WhenDtoIsNull()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.UpdateCarnivalBlockMember(1, null as CarnivalBlockMemberUpdate, 1);

        // Assert
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task UpdateCarnivalBlockMember_ReturnsNotFound_WhenMemberDoesNotExist()
    {
        // Arrange
        var updateDto = new CarnivalBlockMemberUpdate(
            1,
            2,
            RolesEnum.Manager
        );

        _serviceMock.Setup(s => s.UpdateAsync(999, 1, RolesEnum.Manager))
            .ThrowsAsync(new KeyNotFoundException("Carnival block member does not exist."));

        var controller = CreateController();

        // Act
        var result = await controller.UpdateCarnivalBlockMember(999, updateDto, 1);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Carnival block member does not exist.", notFoundResult.Value);
    }

    [Fact]
    public async Task UpdateCarnivalBlockMember_ReturnsUnauthorized_WhenMemberNotAuthorized()
    {
        // Arrange
        var updateDto = new CarnivalBlockMemberUpdate(
            1,
            2,
            RolesEnum.Manager
        );

        _serviceMock.Setup(s => s.UpdateAsync(1, 2, RolesEnum.Manager))
            .ThrowsAsync(new UnauthorizedAccessException("Member is not authorized to update member roles."));

        var controller = CreateController();

        // Act
        var result = await controller.UpdateCarnivalBlockMember(1, updateDto, 2);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("Member is not authorized to update member roles.", unauthorizedResult.Value);
    }

    [Fact]
    public async Task UpdateCarnivalBlockMember_ReturnsBadRequest_WhenTryingToUpdateOwner()
    {
        // Arrange
        var updateDto = new CarnivalBlockMemberUpdate(
            1,
            1,
            RolesEnum.Manager
        );

        _serviceMock.Setup(s => s.UpdateAsync(1, 1, RolesEnum.Manager))
            .ThrowsAsync(new InvalidOperationException("Cannot change the owner's role."));

        var controller = CreateController();

        // Act
        var result = await controller.UpdateCarnivalBlockMember(1, updateDto, 1);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Cannot change the owner's role.", badRequestResult.Value);
    }

    [Fact]
    public async Task DeleteCarnivalBlockMember_ReturnsNoContent_WhenSuccess()
    {
        // Arrange
        _serviceMock.Setup(s => s.DeleteAsync(1, 1)).ReturnsAsync(true);

        var controller = CreateController();

        // Act
        var result = await controller.DeleteCarnivalBlockMember(1, 1);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteCarnivalBlockMember_ReturnsNotFound_WhenMemberDoesNotExist()
    {
        // Arrange
        _serviceMock.Setup(s => s.DeleteAsync(999, 1))
            .ThrowsAsync(new KeyNotFoundException("Carnival block member does not exist."));

        var controller = CreateController();

        // Act
        var result = await controller.DeleteCarnivalBlockMember(999, 1);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Carnival block member does not exist.", notFoundResult.Value);
    }

    [Fact]
    public async Task DeleteCarnivalBlockMember_ReturnsUnauthorized_WhenMemberNotAuthorized()
    {
        // Arrange
        _serviceMock.Setup(s => s.DeleteAsync(1, 2))
            .ThrowsAsync(new UnauthorizedAccessException("Member is not authorized to remove members."));

        var controller = CreateController();

        // Act
        var result = await controller.DeleteCarnivalBlockMember(1, 2);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("Member is not authorized to remove members.", unauthorizedResult.Value);
    }

    [Fact]
    public async Task DeleteCarnivalBlockMember_ReturnsBadRequest_WhenTryingToDeleteOwner()
    {
        // Arrange
        _serviceMock.Setup(s => s.DeleteAsync(1, 1))
            .ThrowsAsync(new InvalidOperationException("Cannot remove the owner from the carnival block."));

        var controller = CreateController();

        // Act
        var result = await controller.DeleteCarnivalBlockMember(1, 1);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Cannot remove the owner from the carnival block.", badRequestResult.Value);
    }
}
