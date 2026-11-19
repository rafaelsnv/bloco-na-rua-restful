using System.Security.Claims;
using BlocoNaRua.Restful.Controllers;
using BlocoNaRua.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BlocoNaRua.Tests.Controllers;

/// <summary>
/// Controller level tests for the API owned auth endpoints.
/// The full Google OAuth roundtrip (/auth/google/challenge + /auth/google/complete) is
/// intentionally NOT covered here: it needs real Google credentials and a browser redirect.
/// What is covered is everything around it — password login/signup, refresh and logout.
/// </summary>
public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock = new();

    private AuthController CreateController()
    {
        var controller = new AuthController(_authServiceMock.Object);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
        return controller;
    }

    [Fact]
    public async Task Login_ReturnsOk_WhenCredentialsAreValid()
    {
        var request = new LoginRequest("test@example.com", "password123");
        _authServiceMock
            .Setup(s => s.LoginAsync(It.IsAny<LoginRequest>()))
            .ReturnsAsync(new LoginResponse("access", "refresh", 3600, "bearer", Guid.NewGuid()));

        var controller = CreateController();

        var result = await controller.Login(request);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenCredentialsAreInvalid()
    {
        var request = new LoginRequest("test@example.com", "wrong-password");
        _authServiceMock
            .Setup(s => s.LoginAsync(It.IsAny<LoginRequest>()))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid credentials"));

        var controller = CreateController();

        var result = await controller.Login(request);

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task Login_ReturnsBadRequest_WhenModelStateInvalid()
    {
        var request = new LoginRequest("", "");
        var controller = CreateController();
        controller.ModelState.AddModelError("Email", "Email is required");

        var result = await controller.Login(request);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Signup_ReturnsCreated_WhenEmailIsNew()
    {
        _authServiceMock
            .Setup(s => s.SignupAsync(It.IsAny<SignupRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(("access", "refresh", 3600));

        var controller = CreateController();

        var result = await controller.Signup(new SignupRequest("test@example.com", "password123", "Test User"));

        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status201Created, statusResult.StatusCode);
        Assert.NotNull(statusResult.Value);
    }

    [Fact]
    public async Task Signup_ReturnsConflict_WhenEmailIsAlreadyRegistered()
    {
        _authServiceMock
            .Setup(s => s.SignupAsync(It.IsAny<SignupRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(((string, string, int)?)null);

        var controller = CreateController();

        var result = await controller.Signup(new SignupRequest("test@example.com", "password123", "Test User"));

        var conflictResult = Assert.IsType<ConflictObjectResult>(result);
        Assert.Equal(StatusCodes.Status409Conflict, conflictResult.StatusCode);
    }

    [Fact]
    public async Task Signup_ReturnsBadRequest_WhenModelStateInvalid()
    {
        var controller = CreateController();
        controller.ModelState.AddModelError("Email", "Invalid email format");

        var result = await controller.Signup(new SignupRequest("not-an-email", "password123", "Test User"));

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Refresh_ReturnsUnauthorized_WhenTokenIsInvalid()
    {
        _authServiceMock
            .Setup(s => s.RotateRefreshTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(((string, string, int)?)null);

        var controller = CreateController();

        var result = await controller.Refresh(new RefreshRequest("invalid-token"));

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task Refresh_ReturnsOk_WhenTokenIsValid()
    {
        _authServiceMock
            .Setup(s => s.RotateRefreshTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(((string, string, int)?)("access", "refresh", 3600));

        var controller = CreateController();

        var result = await controller.Refresh(new RefreshRequest("valid-token"));

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task Refresh_ReturnsUnauthorized_WhenReusingRotatedToken()
    {
        // Model of the rotation flow: only the newest refresh token stays valid.
        var validTokens = new HashSet<string> { "old-token" };

        _authServiceMock
            .Setup(s => s.RotateRefreshTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string token, CancellationToken _) =>
            {
                if (!validTokens.Remove(token))
                {
                    return null;
                }

                validTokens.Add("new-token");
                return ((string, string, int)?)("access", "new-token", 3600);
            });

        var controller = CreateController();

        var first = await controller.Refresh(new RefreshRequest("old-token"));
        Assert.IsType<OkObjectResult>(first);

        var replay = await controller.Refresh(new RefreshRequest("old-token"));
        Assert.IsType<UnauthorizedObjectResult>(replay);
    }

    [Fact]
    public async Task Logout_ReturnsUnauthorized_WhenSubjectClaimIsMissing()
    {
        var controller = CreateController();
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity())
            }
        };

        var result = await controller.Logout();

        Assert.IsType<UnauthorizedObjectResult>(result);
        _authServiceMock.Verify(
            s => s.RevokeRefreshTokenAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
