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

public record SignupRequest(
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    string Email,

    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    string Password,

    [Required(ErrorMessage = "Name is required")]
    string Name
);

public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    string TokenType,
    Guid UserId
);

/// <summary>
/// API owned authentication (plan §1/§3). The Supabase HTTP proxy is gone: the API issues
/// its own tokens through <see cref="ITokenService"/> and the members table is the single
/// identity source of truth.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Password login. The member is looked up by email and the password is checked against
    /// <c>members.password_hash</c>. Every failure (unknown email, wrong password, account of
    /// another provider) raises the same neutral <see cref="UnauthorizedAccessException"/>.
    /// </summary>
    Task<LoginResponse> LoginAsync(LoginRequest request);

    /// <summary>
    /// Creates a password account and issues a token pair.
    /// The email is normalized to lower case and stored as <c>provider = "password"</c>;
    /// <c>provider_id</c> is the lower cased email so it matches the unique provider index.
    /// Returns <c>null</c> when the email is already registered (conflict).
    /// </summary>
    Task<(string AccessToken, string RefreshToken, int ExpiresIn)?> SignupAsync(
        SignupRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Upserts the member behind a Google identity and issues a fresh token pair.
    /// The member is looked up by (<c>provider</c>, <c>providerId</c>); when it does not exist
    /// it is created with <c>provider = "google"</c> and <c>is_active = true</c>.
    /// </summary>
    /// <param name="picture">Optional Google avatar URL; ignored when null/blank.</param>
    Task<(string AccessToken, string RefreshToken, int ExpiresIn)> ProcessGoogleCallbackAsync(
        string displayName,
        string email,
        string providerId,
        CancellationToken ct = default,
        string? picture = null);

    /// <summary>
    /// Rotates the refresh token of <paramref name="refreshToken"/> and returns the new pair.
    /// Returns <c>null</c> when the token is unknown, revoked or expired.
    /// </summary>
    Task<(string AccessToken, string RefreshToken, int ExpiresIn)?> RotateRefreshTokenAsync(
        string refreshToken,
        CancellationToken ct = default);

    /// <summary>Clears the refresh token of the member (logout / revocation).</summary>
    Task RevokeRefreshTokenAsync(Guid memberUuid, CancellationToken ct = default);
}
