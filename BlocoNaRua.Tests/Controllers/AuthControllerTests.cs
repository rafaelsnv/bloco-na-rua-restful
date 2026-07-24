using BlocoNaRua.Restful.Controllers;
using BlocoNaRua.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BlocoNaRua.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock = new();

    private AuthController CreateController() => new(_authServiceMock.Object);

    [Fact]
    public async Task Login_ReturnsOk_WhenCredentialsValid()
    {
        // Arrange
        var request = new LoginRequest("test@example.com", "password123");
        var response = new LoginResponse("access_token", "refresh_token", 3600, "Bearer", Guid.NewGuid());
        _authServiceMock.Setup(s => s.LoginAsync(request)).ReturnsAsync(response);

        var controller = CreateController();

        // Act
        var result = await controller.Login(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedResponse = Assert.IsType<LoginResponse>(okResult.Value);
        Assert.Equal(response.AccessToken, returnedResponse.AccessToken);
        Assert.Equal(response.RefreshToken, returnedResponse.RefreshToken);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenCredentialsInvalid()
    {
        // Arrange
        var request = new LoginRequest("invalid@example.com", "wrongpassword");
        _authServiceMock.Setup(s => s.LoginAsync(request)).ThrowsAsync(new HttpRequestException("Invalid credentials"));

        var controller = CreateController();

        // Act
        var result = await controller.Login(request);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.NotNull(unauthorizedResult.Value);
    }

    [Fact]
    public async Task Login_ReturnsBadRequest_WhenModelStateInvalid()
    {
        // Arrange
        var request = new LoginRequest("", "");
        var controller = CreateController();
        controller.ModelState.AddModelError("Email", "Email is required");

        // Act
        var result = await controller.Login(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }
}
