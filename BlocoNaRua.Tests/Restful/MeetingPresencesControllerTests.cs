using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Restful.Controllers;
using BlocoNaRua.Restful.Models.MeetingPresence;
using BlocoNaRua.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BlocoNaRua.Tests.Restful;

public class MeetingPresencesControllerTests
{
    private readonly Mock<IMeetingPresenceService> _serviceMock = new();

    private MeetingPresencesController CreateController() => new(_serviceMock.Object);

    [Fact]
    public async Task GetAll_ReturnsOkWithList()
    {
        // Arrange
        var entities = new List<MeetingPresenceEntity>
        {
            new(
                id: 1
            )
            {
                MemberId = 1,
                MeetingId = 1,
                CarnivalBlockId = 1,
                IsPresent = true
            }
        };
        _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(entities);

        // Act
        var controller = CreateController();
        var result = await controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsAssignableFrom<IList<MeetingPresenceResponse>>(okResult.Value);
    }

    [Fact]
    public async Task GetById_ReturnsOkWithEntity()
    {
        // Arrange
        var entity = new MeetingPresenceEntity(
            id: 1
        )
        {
            MemberId = 1,
            MeetingId = 1,
            CarnivalBlockId = 1,
            IsPresent = true
        };
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(entity);

        // Act
        var controller = CreateController();
        var result = await controller.GetById(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<MeetingPresenceResponse>(okResult.Value);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync((MeetingPresenceEntity?)null);

        // Act
        var controller = CreateController();
        var result = await controller.GetById(1);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtAction()
    {
        // Arrange
        var createDto = new MeetingPresenceCreate(
            MemberId: 1,
            MeetingId: 1,
            CarnivalBlockId: 1,
            IsPresent: true
        );
        var entity = new MeetingPresenceEntity(
            id: 1
        )
        {
            MemberId = createDto.MemberId,
            MeetingId = createDto.MeetingId,
            CarnivalBlockId = createDto.CarnivalBlockId,
            IsPresent = createDto.IsPresent
        };
        _serviceMock.Setup(s => s.CreateAsync(It.IsAny<MeetingPresenceEntity>(), It.IsAny<int>())).ReturnsAsync(entity);

        // Act
        var controller = CreateController();
        var result = await controller.Create(createDto, 1);

        // Assert
        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal("GetById", created.ActionName);
        Assert.IsType<MeetingPresenceResponse>(created.Value);
    }

    [Fact]
    public async Task Create_ReturnsNotFound_WhenKeyNotFoundException()
    {
        // Arrange
        _serviceMock.Setup(s => s.CreateAsync(It.IsAny<MeetingPresenceEntity>(), It.IsAny<int>())).ThrowsAsync(new KeyNotFoundException("Meeting does not exist."));
        var createDto = new MeetingPresenceCreate(
            MemberId: 1,
            MeetingId: 1,
            CarnivalBlockId: 1,
            IsPresent: true
        );

        // Act
        var controller = CreateController();
        var result = await controller.Create(createDto, 1);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Meeting does not exist.", notFoundResult.Value);
    }

    [Fact]
    public async Task Create_ReturnsUnauthorized_WhenUnauthorizedAccessException()
    {
        // Arrange
        _serviceMock.Setup(s => s.CreateAsync(It.IsAny<MeetingPresenceEntity>(), It.IsAny<int>())).ThrowsAsync(new UnauthorizedAccessException("You are not authorized to create a meeting presence for another member."));
        var createDto = new MeetingPresenceCreate(
            MemberId: 2,
            MeetingId: 1,
            CarnivalBlockId: 1,
            IsPresent: true
        );

        // Act
        var controller = CreateController();
        var result = await controller.Create(createDto, 1);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("You are not authorized to create a meeting presence for another member.", unauthorizedResult.Value);
    }

    [Fact]
    public async Task Update_ReturnsOk_WhenSuccess()
    {
        // Arrange
        var updatedEntity = new MeetingPresenceEntity(
            id: 1
        )
        {
            MemberId = 1,
            MeetingId = 1,
            CarnivalBlockId = 1,
            IsPresent = false
        };
        _serviceMock.Setup(s => s.UpdateAsync(It.IsAny<int>(), It.IsAny<MeetingPresenceEntity>(), It.IsAny<int>())).ReturnsAsync(updatedEntity);
        var updateDto = new MeetingPresenceUpdate(
            IsPresent: false
        );

        // Act
        var controller = CreateController();
        var result = await controller.Update(1, updateDto, 1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<MeetingPresenceResponse>(okResult.Value);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenKeyNotFoundException()
    {
        // Arrange
        _serviceMock.Setup(s => s.UpdateAsync(It.IsAny<int>(), It.IsAny<MeetingPresenceEntity>(), It.IsAny<int>())).ThrowsAsync(new KeyNotFoundException("Meeting presence does not exist."));
        var updateDto = new MeetingPresenceUpdate(
            IsPresent: false
        );

        // Act
        var controller = CreateController();
        var result = await controller.Update(1, updateDto, 1);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Meeting presence does not exist.", notFoundResult.Value);
    }

    [Fact]
    public async Task Update_ReturnsUnauthorized_WhenUnauthorizedAccessException()
    {
        // Arrange
        _serviceMock.Setup(s => s.UpdateAsync(It.IsAny<int>(), It.IsAny<MeetingPresenceEntity>(), It.IsAny<int>())).ThrowsAsync(new UnauthorizedAccessException("You are not authorized to update this meeting presence."));
        var updateDto = new MeetingPresenceUpdate(
            IsPresent: false
        );

        // Act
        var controller = CreateController();
        var result = await controller.Update(1, updateDto, 1);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("You are not authorized to update this meeting presence.", unauthorizedResult.Value);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenSuccess()
    {
        // Arrange
        _serviceMock.Setup(s => s.DeleteAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(true);

        // Act
        var controller = CreateController();
        var result = await controller.Delete(1, 1);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenKeyNotFoundException()
    {
        // Arrange
        _serviceMock.Setup(s => s.DeleteAsync(It.IsAny<int>(), It.IsAny<int>())).ThrowsAsync(new KeyNotFoundException("Meeting presence does not exist."));

        // Act
        var controller = CreateController();
        var result = await controller.Delete(1, 1);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Meeting presence does not exist.", notFoundResult.Value);
    }

    [Fact]
    public async Task Delete_ReturnsUnauthorized_WhenUnauthorizedAccessException()
    {
        // Arrange
        _serviceMock.Setup(s => s.DeleteAsync(It.IsAny<int>(), It.IsAny<int>())).ThrowsAsync(new UnauthorizedAccessException("You are not authorized to delete this meeting presence."));

        // Act
        var controller = CreateController();
        var result = await controller.Delete(1, 1);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("You are not authorized to delete this meeting presence.", unauthorizedResult.Value);
    }
}
