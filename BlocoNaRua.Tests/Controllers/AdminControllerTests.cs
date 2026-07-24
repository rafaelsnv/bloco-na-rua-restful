using BlocoNaRua.Restful.Controllers;
using BlocoNaRua.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BlocoNaRua.Tests.Controllers;

public class AdminControllerTests
{
    private readonly Mock<IAdminService> _serviceMock = new();

    private AdminController CreateController() => new(_serviceMock.Object);

    [Fact]
    public async Task DeleteSignupCleanup_ReturnsOk_WhenDeletionSucceeds()
    {
        // Arrange
        var uuid = Guid.NewGuid();
        _serviceMock.Setup(s => s.DeleteSignupAsync(uuid)).ReturnsAsync((true, null));

        var controller = CreateController();

        // Act
        var result = await controller.DeleteSignupCleanup(uuid);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        var deleted = okResult.Value.GetType().GetProperty("deleted");
        Assert.NotNull(deleted);
        Assert.Equal(true, deleted.GetValue(okResult.Value));
    }

    [Fact]
    public async Task DeleteSignupCleanup_ReturnsNotFound_WhenDeletionFails()
    {
        // Arrange
        var uuid = Guid.NewGuid();
        var errorMessage = "User not found";
        _serviceMock.Setup(s => s.DeleteSignupAsync(uuid)).ReturnsAsync((false, errorMessage));

        var controller = CreateController();

        // Act
        var result = await controller.DeleteSignupCleanup(uuid);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(errorMessage, notFoundResult.Value);
    }

    [Fact]
    public async Task DeleteSignupCleanup_Returns500_WhenHttpRequestException()
    {
        // Arrange
        var uuid = Guid.NewGuid();
        var errorDetail = "Supabase connection failed";
        _serviceMock.Setup(s => s.DeleteSignupAsync(uuid)).ThrowsAsync(new HttpRequestException(errorDetail));

        var controller = CreateController();

        // Act
        var result = await controller.DeleteSignupCleanup(uuid);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusResult.StatusCode);
        Assert.NotNull(statusResult.Value);
        var statusCodeProp = statusResult.Value.GetType().GetProperty("statusCode");
        var messageProp = statusResult.Value.GetType().GetProperty("message");
        var detailProp = statusResult.Value.GetType().GetProperty("detail");
        Assert.NotNull(statusCodeProp);
        Assert.NotNull(messageProp);
        Assert.NotNull(detailProp);
        Assert.Equal(500, statusCodeProp.GetValue(statusResult.Value));
        Assert.Equal("Supabase error", messageProp.GetValue(statusResult.Value));
        Assert.Equal(errorDetail, detailProp.GetValue(statusResult.Value));
    }
}