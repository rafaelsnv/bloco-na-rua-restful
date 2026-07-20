using System.ComponentModel.DataAnnotations;

namespace BlocoNaRua.Services.Interfaces;

public record LoginRequest(
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    string Email,

    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    string Password
);

public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    string TokenType,
    Guid UserId
);

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
}
