using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Restful.Controllers;
using BlocoNaRua.Restful.Models.Meeting;
using BlocoNaRua.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BlocoNaRua.Tests.Restful;

public class MeetingsControllerTests
{
    private readonly Mock<IMeetingService> _serviceMock = new();

    private MeetingsController CreateController() => new(_serviceMock.Object);

    [Fact]
    public async Task GetAll_ReturnsOkWithList()
    {
        // Arrange
        var entities = new List<MeetingEntity>
        {
            new(
                id: 1,
                name: "Test Meeting",
                description: "Description",
                location: "Location",
                meetingCode: "code123",
                meetingDateTime: DateTime.Now,
                carnivalBlockId: 1
            )
        };
        _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(entities);

        // Act
        var controller = CreateController();
        var result = await controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsAssignableFrom<IList<MeetingResponse>>(okResult.Value);
    }

    [Fact]
    public async Task GetAllByBlockId_ReturnsOkWithList()
    {
        // Arrange
        var entities = new List<MeetingEntity>
        {
            new(
                id: 1,
                name: "Test Meeting",
                description: "Description",
                location: "Location",
                meetingCode: "code123",
                meetingDateTime: DateTime.Now,
                carnivalBlockId: 1
            )
        };
        _serviceMock.Setup(s => s.GetAllByBlockIdAsync(1)).ReturnsAsync(entities);

        // Act
        var controller = CreateController();
        var result = await controller.GetAllByBlockId(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsAssignableFrom<IList<MeetingResponse>>(okResult.Value);
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtAction()
    {
        // Arrange
        var createDto = new MeetingCreate(
            Name: "New Meeting",
            Description: "Desc",
            Location: "Loc",
            MeetingDateTime: DateTime.Now,
            CarnivalBlockId: 1
        );
        var entity = new MeetingEntity(
            id: 1,
            name: createDto.Name,
            description: createDto.Description,
            location: createDto.Location,
            meetingCode: "code",
            meetingDateTime: createDto.MeetingDateTime,
            carnivalBlockId: createDto.CarnivalBlockId
        );
        _serviceMock.Setup(s => s.CreateAsync(It.IsAny<MeetingEntity>(), It.IsAny<int>())).ReturnsAsync(entity);

        // Act
        var controller = CreateController();
        var result = await controller.Create(createDto, 1);

        // Assert
        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal("GetAllByBlockId", created.ActionName);
        Assert.IsType<MeetingResponse>(created.Value);
    }

    [Fact]
    public async Task Create_ReturnsNotFound_WhenKeyNotFoundException()
    {
        // Arrange
        _serviceMock.Setup(s => s.CreateAsync(It.IsAny<MeetingEntity>(), It.IsAny<int>())).ThrowsAsync(new KeyNotFoundException("Carnival block does not exist."));
        var createDto = new MeetingCreate(
            Name: "New Meeting",
            Description: "Desc",
            Location: "Loc",
            MeetingDateTime: DateTime.Now,
            CarnivalBlockId: 1
        );

        // Act
        var controller = CreateController();
        var result = await controller.Create(createDto, 1);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Carnival block does not exist.", notFoundResult.Value);
    }

    [Fact]
    public async Task Create_ReturnsUnauthorized_WhenUnauthorizedAccessException()
    {
        // Arrange
        _serviceMock.Setup(s => s.CreateAsync(It.IsAny<MeetingEntity>(), It.IsAny<int>())).ThrowsAsync(new UnauthorizedAccessException("Member is not authorized to create a meeting for this carnival block."));
        var createDto = new MeetingCreate(
            Name: "New Meeting",
            Description: "Desc",
            Location: "Loc",
            MeetingDateTime: DateTime.Now,
            CarnivalBlockId: 1
        );

        // Act
        var controller = CreateController();
        var result = await controller.Create(createDto, 1);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("Member is not authorized to create a meeting for this carnival block.", unauthorizedResult.Value);
    }

    [Fact]
    public async Task Update_ReturnsOk_WhenSuccess()
    {
        // Arrange
        var updatedEntity = new MeetingEntity(
            id: 1,
            name: "Updated Meeting",
            description: "Updated Desc",
            location: "Updated Loc",
            meetingCode: "code",
            meetingDateTime: DateTime.Now,
            carnivalBlockId: 1
        );
        _serviceMock.Setup(s => s.UpdateAsync(It.IsAny<int>(), It.IsAny<MeetingEntity>(), It.IsAny<int>())).ReturnsAsync(updatedEntity);
        var updateDto = new MeetingUpdate(
            Name: "Updated Meeting",
            Description: "Updated Desc",
            Location: "Updated Loc",
            MeetingDateTime: DateTime.Now
        );

        // Act
        var controller = CreateController();
        var result = await controller.Update(1, updateDto, 1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<MeetingResponse>(okResult.Value);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenKeyNotFoundException()
    {
        // Arrange
        _serviceMock.Setup(s => s.UpdateAsync(It.IsAny<int>(), It.IsAny<MeetingEntity>(), It.IsAny<int>())).ThrowsAsync(new KeyNotFoundException("Meeting does not exist."));
        var updateDto = new MeetingUpdate(
            Name: "Updated Meeting",
            Description: "Updated Desc",
            Location: "Updated Loc",
            MeetingDateTime: DateTime.Now
        );

        // Act
        var controller = CreateController();
        var result = await controller.Update(1, updateDto, 1);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Meeting does not exist.", notFoundResult.Value);
    }

    [Fact]
    public async Task Update_ReturnsUnauthorized_WhenUnauthorizedAccessException()
    {
        // Arrange
        _serviceMock.Setup(s => s.UpdateAsync(It.IsAny<int>(), It.IsAny<MeetingEntity>(), It.IsAny<int>())).ThrowsAsync(new UnauthorizedAccessException("Member is not authorized to update this meeting."));
        var updateDto = new MeetingUpdate(
            Name: "Updated Meeting",
            Description: "Updated Desc",
            Location: "Updated Loc",
            MeetingDateTime: DateTime.Now
        );

        // Act
        var controller = CreateController();
        var result = await controller.Update(1, updateDto, 1);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("Member is not authorized to update this meeting.", unauthorizedResult.Value);
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
        _serviceMock.Setup(s => s.DeleteAsync(It.IsAny<int>(), It.IsAny<int>())).ThrowsAsync(new KeyNotFoundException("Meeting does not exist."));

        // Act
        var controller = CreateController();
        var result = await controller.Delete(1, 1);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Meeting does not exist.", notFoundResult.Value);
    }

    [Fact]
    public async Task Delete_ReturnsUnauthorized_WhenUnauthorizedAccessException()
    {
        // Arrange
        _serviceMock.Setup(s => s.DeleteAsync(It.IsAny<int>(), It.IsAny<int>())).ThrowsAsync(new UnauthorizedAccessException("Member is not authorized to delete this meeting."));

        // Act
        var controller = CreateController();
        var result = await controller.Delete(1, 1);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("Member is not authorized to delete this meeting.", unauthorizedResult.Value);
    }
}
