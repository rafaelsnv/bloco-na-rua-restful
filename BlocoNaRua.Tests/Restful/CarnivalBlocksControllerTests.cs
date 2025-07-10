using BlocoNaRua.Data.Repositories.Interfaces;
using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Restful.Controllers;
using BlocoNaRua.Restful.Models.CarnivalBlock;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BlocoNaRua.Tests.Restful;

public class CarnivalBlocksControllerTests
{
    private readonly Mock<ILogger<CarnivalBlocksController>> _loggerMock = new();
    private readonly Mock<ICarnivalBlocksRepository> _repoMock = new();

    private CarnivalBlocksController CreateController()
        => new(_loggerMock.Object, _repoMock.Object);

    [Fact]
    public async Task GetAllCarnivalBlocks_Success()
    {
        _repoMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync([
                new
                (
                    id: 1,
                    name: "Test Block",
                    ownerId: 1,
                    inviteCode: "invite123",
                    managersInviteCode: "manager123",
                    carnivalBlockImage: "block_image.jpg"
                )
            ]);

        var controller = CreateController();
        var result = await controller.GetAllCarnivalBlocks();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsAssignableFrom<IEnumerable<CarnivalBlockEntity>>(okResult.Value);
    }

    [Fact]
    public async Task GetCarnivalBlockById_Success()
    {
        var entity = new CarnivalBlockEntity
        (
            id: 1,
            name: "Test Block",
            ownerId: 1,
            inviteCode: "invite123",
            managersInviteCode: "manager123",
            carnivalBlockImage: "block_image.jpg"
        );
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(entity);

        var controller = CreateController();
        var result = await controller.GetCarnivalBlockById(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(entity, okResult.Value);
    }

    [Fact]
    public async Task GetCarnivalBlockById_NotFound()
    {
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((CarnivalBlockEntity)null);

        var controller = CreateController();
        var result = await controller.GetCarnivalBlockById(1);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task CreateCarnivalBlock_BadRequest()
    {
        var controller = CreateController();
        var result = await controller.CreateCarnivalBlock(null);

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task CreateCarnivalBlock_Success()
    {
        var createDto = new CarnivalBlockCreate
        (
            Name: "Test",
            OwnerId: 1,
            CarnivalBlockImage: "img"
        );
        var entity = new CarnivalBlockEntity
        (
            id: 1,
            name: createDto.Name,
            ownerId: createDto.OwnerId,
            inviteCode: string.Empty,
            managersInviteCode: string.Empty,
            carnivalBlockImage: createDto.CarnivalBlockImage
        );
        _repoMock.Setup(r => r.AddAsync(It.IsAny<CarnivalBlockEntity>())).ReturnsAsync(entity);

        var controller = CreateController();
        var result = await controller.CreateCarnivalBlock(createDto);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal("GetCarnivalBlockById", created.ActionName);
        Assert.Equal(entity, created.Value);
    }

    [Fact]
    public async Task UpdateCarnivalBlock_NotFound()
    {
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((CarnivalBlockEntity)null);

        var controller = CreateController();
        var updateDto = new CarnivalBlockUpdate
        (
            Name: "Test",
            MemberId: 1,
            CarnivalBlockImage: "img"
        );
        var result = await controller.UpdateCarnivalBlock(1, updateDto);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task UpdateCarnivalBlock_BadRequest()
    {
        var controller = CreateController();
        var result = await controller.UpdateCarnivalBlock(1, null);

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task UpdateCarnivalBlock_Success()
    {
        var entity = new CarnivalBlockEntity
        (
            id: 1,
            name: "Name",
            ownerId: 1,
            inviteCode: string.Empty,
            managersInviteCode: string.Empty,
            carnivalBlockImage: "img"
        );
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(entity);

        var controller = CreateController();
        var updateDto = new CarnivalBlockUpdate
        (
            Name: "Updated",
            MemberId: 2,
            CarnivalBlockImage: "img"
        );

        var result = await controller.UpdateCarnivalBlock(1, updateDto);

        Assert.IsType<NoContentResult>(result);
        _repoMock.Verify(r => r.UpdateAsync(updateDto.MemberId, entity), Times.Once);
    }

    [Fact]
    public async Task DeleteCarnivalBlock_NotFound()
    {
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((CarnivalBlockEntity)null);

        var controller = CreateController();
        var result = await controller.DeleteCarnivalBlock(1);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteCarnivalBlock_Success()
    {
        var entity = new CarnivalBlockEntity
        (
            id: 1,
            name: "Name",
            ownerId: 1,
            inviteCode: string.Empty,
            managersInviteCode: string.Empty,
            carnivalBlockImage: "img"
        );
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(entity);

        var controller = CreateController();
        var result = await controller.DeleteCarnivalBlock(1);

        Assert.IsType<NoContentResult>(result);
        _repoMock.Verify(r => r.DeleteAsync(entity), Times.Once);
    }
}
