using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Domain.Enums;
using BlocoNaRua.Restful.Controllers;
using BlocoNaRua.Restful.Models.CarnivalBlock;
using BlocoNaRua.Restful.Models.Meeting;
using BlocoNaRua.Restful.Models.Member;
using BlocoNaRua.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BlocoNaRua.Tests.Restful;

public class MembersControllerTests
{
    private readonly Mock<IMembersService> _serviceMock = new();

    private MembersController CreateController() => new(_serviceMock.Object);

    [Fact]
    public async Task GetAll_ReturnsOkWithList()
    {
        // Arrange
        var entities = new List<MemberEntity>
        {
            new(1, "Member 1", "member1@test.com", "111", "img1.jpg", new Guid()),
            new(2, "Member 2", "member2@test.com", "222", "img2.jpg", new Guid())
        };
        _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(entities);

        var controller = CreateController();

        // Act
        var result = await controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var dtoList = Assert.IsAssignableFrom<IList<MemberResponse>>(okResult.Value);
        Assert.Equal(2, dtoList.Count);
    }

    [Fact]
    public async Task GetById_ReturnsOkWithEntity()
    {
        // Arrange
        var entity = new MemberEntity(1, "Test Member", "test@test.com", "123", "img.jpg", new Guid());
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(entity);

        var controller = CreateController();

        // Act
        var result = await controller.GetById(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<MemberResponse>(okResult.Value);
        Assert.Equal(1, dto.Id);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync((MemberEntity?)null);

        var controller = CreateController();

        // Act
        var result = await controller.GetById(1);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtAction()
    {
        // Arrange
        var createDto = new MemberCreate("Test", "test@test.com", "123", "img.jpg", new Guid().ToString());
        var entity = new MemberEntity(1, createDto.Name, createDto.Email, createDto.Phone, createDto.ProfileImage, new Guid(createDto.Uuid));
        _serviceMock.Setup(s => s.CreateAsync(It.IsAny<MemberEntity>())).ReturnsAsync(entity);

        var controller = CreateController();

        // Act
        var result = await controller.Create(createDto);

        // Assert
        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal("GetById", created.ActionName);
        var dto = Assert.IsType<MemberResponse>(created.Value);
        Assert.Equal(1, dto.Id);
    }

    [Fact]
    public async Task GetMemberCarnivalBlocks_ReturnsOkWithBlocks()
    {
        // Arrange
        var memberId = 1;
        var blockMembers = new List<CarnivalBlockMembersEntity>
        {
            new(1, 10, memberId, RolesEnum.Member)
            {
                CarnivalBlock = new CarnivalBlockEntity(10, 1, "Bloco Teste", "", "", "")
            }
        };
        _serviceMock.Setup(s => s.GetMemberBlocksAsync(memberId)).ReturnsAsync(blockMembers);
        var controller = CreateController();

        // Act
        var result = await controller.GetMemberCarnivalBlocks(memberId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var dtoList = Assert.IsAssignableFrom<IList<CarnivalBlockResponse>>(okResult.Value);
        Assert.Single(dtoList);
        Assert.Equal(10, dtoList[0].Id);
    }

    [Fact]
    public async Task GetMemberCarnivalBlocks_ReturnsNotFound_WhenNoBlocks()
    {
        // Arrange
        var memberId = 1;
        _serviceMock.Setup(s => s.GetMemberBlocksAsync(memberId)).ReturnsAsync(new List<CarnivalBlockMembersEntity>());
        var controller = CreateController();

        // Act
        var result = await controller.GetMemberCarnivalBlocks(memberId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetMemberMeetings_ReturnsOkWithMeetings()
    {
        // Arrange
        var memberId = 1;
        var meetings = new List<MeetingEntity>
        {
            new(1, "Meeting 1", "", "", "", DateTime.Now, 10)
        };
        _serviceMock.Setup(s => s.GetMemberMeetingsAsync(memberId)).ReturnsAsync(meetings);
        var controller = CreateController();

        // Act
        var result = await controller.GetMemberMeetings(memberId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var dtoList = Assert.IsAssignableFrom<IList<MeetingResponse>>(okResult.Value);
        Assert.Single(dtoList);
        Assert.Equal("Meeting 1", dtoList[0].Name);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenEntityDoesNotExist()
    {
        // Arrange
        _serviceMock.Setup(s => s.UpdateAsync(1, 1, It.IsAny<MemberEntity>())).ReturnsAsync((MemberEntity?)null);
        var updateDto = new MemberUpdate("Test", "test@test.com", "123", "img.jpg");
        var controller = CreateController();

        // Act
        var result = await controller.Update(1, updateDto, 1);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Update_ReturnsUnauthorized_WhenRequesterIsNotTarget()
    {
        // Arrange
        _serviceMock.Setup(s => s.UpdateAsync(1, 2, It.IsAny<MemberEntity>()))
            .ThrowsAsync(new UnauthorizedAccessException("Member is not authorized to update this resource."));
        var updateDto = new MemberUpdate("Test", "test@test.com", "123", "img.jpg");
        var controller = CreateController();

        // Act
        var result = await controller.Update(1, updateDto, 2);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("Member is not authorized to update this resource.", unauthorizedResult.Value);
    }

    [Fact]
    public async Task Update_ReturnsOk_WhenSuccess()
    {
        // Arrange
        var updatedEntity = new MemberEntity(1, "Updated", "updated@test.com", "321", "updated.jpg", new Guid());
        _serviceMock.Setup(s => s.UpdateAsync(1, 1, It.IsAny<MemberEntity>())).ReturnsAsync(updatedEntity);
        var updateDto = new MemberUpdate("Updated", "updated@test.com", "321", "updated.jpg");
        var controller = CreateController();

        // Act
        var result = await controller.Update(1, updateDto, 1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<MemberResponse>(okResult.Value);
        Assert.Equal("Updated", dto.Name);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenEntityDoesNotExist()
    {
        // Arrange
        _serviceMock.Setup(s => s.DeleteAsync(1, 1)).ReturnsAsync(false);
        var controller = CreateController();

        // Act
        var result = await controller.Delete(1, 1);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_ReturnsUnauthorized_WhenRequesterIsNotTarget()
    {
        // Arrange
        _serviceMock.Setup(s => s.DeleteAsync(1, 2))
            .ThrowsAsync(new UnauthorizedAccessException("Member is not authorized to delete this resource."));
        var controller = CreateController();

        // Act
        var result = await controller.Delete(1, 2);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("Member is not authorized to delete this resource.", unauthorizedResult.Value);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenSuccess()
    {
        // Arrange
        _serviceMock.Setup(s => s.DeleteAsync(1, 1)).ReturnsAsync(true);
        var controller = CreateController();

        // Act
        var result = await controller.Delete(1, 1);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }
}
