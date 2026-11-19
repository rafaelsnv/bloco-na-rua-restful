using System.Security.Claims;
using BlocoNaRua.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlocoNaRua.Restful.Controllers;

/// <summary>
/// Request body of the token refresh endpoint.
/// </summary>
/// <param name="RefreshToken">The refresh token issued by a previous login/callback.</param>
public record RefreshRequest(string RefreshToken);

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[AllowAnonymous]
public class AuthController(IAuthService authService) : ControllerBase
{
    private readonly IAuthService _authService = authService;

    /// <summary>
    /// Password login.
    /// </summary>
    [HttpPost("login")]
    [Consumes("application/x-www-form-urlencoded")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromForm] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _authService.LoginAsync(request);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (HttpRequestException ex)
        {
            return Unauthorized(new { message = "Invalid credentials", detail = ex.Message });
        }
    }

    /// <summary>
    /// Creates a password account and returns a fresh token pair.
    /// </summary>
    /// <remarks>
    /// The email is normalized to lower case. A duplicate email answers 409.
    /// </remarks>
    [HttpPost("signup")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Signup([FromBody] SignupRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _authService.SignupAsync(request, HttpContext.RequestAborted);
        if (result is null)
        {
            return Conflict(new { message = "Email already registered" });
        }

        var (accessToken, refreshToken, expiresIn) = result.Value;
        return StatusCode(
            StatusCodes.Status201Created,
            new
            {
                access_token = accessToken,
                refresh_token = refreshToken,
                expires_in = expiresIn,
                token_type = "bearer"
            });
    }

    /// <summary>
    /// Starts the Google OAuth flow. The provider redirects back to
    /// <c>/api/v1/auth/google/complete</c>.
    /// </summary>
    /// <remarks>
    /// The Google scheme is only registered when the server has Google credentials
    /// configured; without them the endpoint answers 503 instead of throwing.
    /// </remarks>
    [HttpGet("google/challenge")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GoogleChallenge()
    {
        var schemeProvider = HttpContext.RequestServices.GetRequiredService<IAuthenticationSchemeProvider>();
        if (await schemeProvider.GetSchemeAsync("Google") is null)
        {
            return Problem(
                statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Google sign-in is not configured");
        }

        return Challenge(new AuthenticationProperties { RedirectUri = "/api/v1/auth/google/complete" }, "Google");
    }

    /// <summary>
    /// Completes the Google OAuth flow and issues an API token pair.
    /// </summary>
    [HttpGet("google/complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GoogleComplete()
    {
        var schemeProvider = HttpContext.RequestServices.GetRequiredService<IAuthenticationSchemeProvider>();
        if (await schemeProvider.GetSchemeAsync("GoogleState") is null)
        {
            return Problem(
                statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Google sign-in is not configured");
        }

        var result = await HttpContext.AuthenticateAsync("GoogleState");
        if (!result.Succeeded || result.Principal is null)
        {
            return Unauthorized(new { message = "Google authentication failed" });
        }

        var sub = result.Principal.FindFirst("sub")?.Value ?? result.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(sub))
        {
            return BadRequest("Google account has no subject claim");
        }

        var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value ?? result.Principal.FindFirst("email")?.Value;
        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest("Google account has no email claim");
        }

        var name = result.Principal.FindFirst(ClaimTypes.Name)?.Value
            ?? result.Principal.FindFirst("name")?.Value
            ?? string.Empty;

        var picture = result.Principal.FindFirst("urn:google:picture")?.Value
            ?? result.Principal.FindFirst("picture")?.Value;

        // The Google state cookie is single use; drop it before issuing tokens.
        await HttpContext.SignOutAsync("GoogleState");

        try
        {
            var (accessToken, refreshToken, expiresIn) = await _authService.ProcessGoogleCallbackAsync(
                name,
                email,
                sub,
                HttpContext.RequestAborted,
                picture);

            return Ok(new
            {
                access_token = accessToken,
                refresh_token = refreshToken,
                expires_in = expiresIn,
                token_type = "bearer"
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Rotates a refresh token and returns a new token pair.
    /// </summary>
    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(request?.RefreshToken))
        {
            return BadRequest(ModelState);
        }

        var result = await _authService.RotateRefreshTokenAsync(request.RefreshToken, HttpContext.RequestAborted);
        if (result is null)
        {
            return Unauthorized(new { message = "Invalid refresh token" });
        }

        var (accessToken, refreshToken, expiresIn) = result.Value;
        return Ok(new
        {
            access_token = accessToken,
            refresh_token = refreshToken,
            expires_in = expiresIn,
            token_type = "bearer"
        });
    }

    /// <summary>
    /// Revokes the refresh token of the authenticated member.
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Logout()
    {
        var sub = User.FindFirst("sub")?.Value;
        if (!Guid.TryParse(sub, out var memberUuid))
        {
            return Unauthorized(new { message = "Invalid token subject" });
        }

        await _authService.RevokeRefreshTokenAsync(memberUuid, HttpContext.RequestAborted);
        return Ok(new { revoked = true });
    }
}
