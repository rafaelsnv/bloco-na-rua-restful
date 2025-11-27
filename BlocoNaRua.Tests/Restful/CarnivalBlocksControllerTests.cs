using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Restful.Controllers;
using BlocoNaRua.Restful.Models.CarnivalBlock;
using BlocoNaRua.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BlocoNaRua.Tests.Restful;

public class CarnivalBlocksControllerTests
{
    private readonly Mock<ICarnivalBlockService> _serviceMock = new();

    private CarnivalBlocksController CreateController() => new(_serviceMock.Object);

    [Fact]
    public async Task GetAll_ReturnsOkWithList()
    {
        var entities = new List<CarnivalBlockEntity>
        {
            new(
                id: 1,
                ownerId: 1,
                name: "Test Block",
                inviteCode: "invite123",
                managersInviteCode: "manager123",
                carnivalBlockImage: "block_image.jpg"
            )
        };
        _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(entities);

        var controller = CreateController();
        var result = await controller.GetAll();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsAssignableFrom<IList<CarnivalBlockResponse>>(okResult.Value);
    }

    [Fact]
    public async Task GetById_ReturnsOkWithEntity()
    {
        var entity = new CarnivalBlockEntity(
            id: 1,
            ownerId: 1,
            name: "Test Block",
            inviteCode: "invite123",
            managersInviteCode: "manager123",
            carnivalBlockImage: "block_image.jpg"
        );
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(entity);

        var controller = CreateController();
        var result = await controller.GetById(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<CarnivalBlockResponse>(okResult.Value);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync((CarnivalBlockEntity?)null);

        var controller = CreateController();
        var result = await controller.GetById(1);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtAction()
    {
        var createDto = new CarnivalBlockCreate(
            Name: "Test",
            OwnerId: 1,
            CarnivalBlockImage: "img"
        );
        var entity = new CarnivalBlockEntity(
            id: 1,
            ownerId: createDto.OwnerId,
            name: createDto.Name,
            inviteCode: string.Empty,
            managersInviteCode: string.Empty,
            carnivalBlockImage: createDto.CarnivalBlockImage
        );
        _serviceMock.Setup(s => s.CreateAsync(It.IsAny<CarnivalBlockEntity>())).ReturnsAsync(entity);

        var controller = CreateController();
        var result = await controller.Create(createDto);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal("GetById", created.ActionName);
        Assert.IsType<CarnivalBlockResponse>(created.Value);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenEntityDoesNotExist()
    {
        _serviceMock.Setup(s => s.UpdateAsync(1, 1, It.IsAny<CarnivalBlockEntity>())).ThrowsAsync(new KeyNotFoundException());

        var controller = CreateController();
        var updateDto = new CarnivalBlockUpdate(
            Name: "Test",
            CarnivalBlockImage: "img"
        );
        var result = await controller.Update(1, updateDto, 1);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Update_ReturnsUnauthorized_WhenMemberIsNotAuthorized()
    {
        _serviceMock.Setup(s => s.UpdateAsync(1, 1, It.IsAny<CarnivalBlockEntity>()))
            .ThrowsAsync(new UnauthorizedAccessException("Member is not authorized to update this carnival block."));

        var controller = CreateController();
        var updateDto = new CarnivalBlockUpdate(
            Name: "Test",
            CarnivalBlockImage: "img"
        );
        var result = await controller.Update(1, updateDto, 1);

        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("Member is not authorized to update this carnival block.", unauthorizedResult.Value);
    }

    [Fact]
    public async Task Update_ReturnsOk_WhenSuccess()
    {
        var updatedEntity = new CarnivalBlockEntity(
            id: 1,
            ownerId: 2,
            name: "Updated",
            inviteCode: string.Empty,
            managersInviteCode: string.Empty,
            carnivalBlockImage: "img"
        );
        _serviceMock.Setup(s => s.UpdateAsync(1, 2, It.IsAny<CarnivalBlockEntity>())).ReturnsAsync(updatedEntity);

        var controller = CreateController();
        var updateDto = new CarnivalBlockUpdate(
            Name: "Updated",
            CarnivalBlockImage: "img"
        );

        var result = await controller.Update(1, updateDto, 2);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<CarnivalBlockResponse>(okResult.Value);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenEntityDoesNotExist()
    {
        _serviceMock.Setup(s => s.DeleteAsync(1, It.IsAny<int>())).ThrowsAsync(new KeyNotFoundException());

        var controller = CreateController();
        var result = await controller.Delete(1, 1);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Delete_ReturnsUnauthorized_WhenMemberIsNotAuthorized()
    {
        _serviceMock.Setup(s => s.DeleteAsync(1, It.IsAny<int>()))
            .ThrowsAsync(new UnauthorizedAccessException("Member is not authorized to delete this carnival block."));

        var controller = CreateController();
        var result = await controller.Delete(1, 1);

        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("Member is not authorized to delete this carnival block.", unauthorizedResult.Value);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenSuccess()
    {
        _serviceMock.Setup(s => s.DeleteAsync(1, It.IsAny<int>())).ReturnsAsync(true);

        var controller = CreateController();
        var result = await controller.Delete(1, 1);

        Assert.IsType<NoContentResult>(result);
    }
}
