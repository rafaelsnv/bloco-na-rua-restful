namespace BlocoNaRua.Services.Interfaces;

/// <summary>
/// Stores and removes user uploaded images on the local upload directory (volume mount).
/// </summary>
public interface IImageUrlService
{
    /// <summary>
    /// Validates and stores an image stream, returning the public relative URL (e.g. "/uploads/{guid}{ext}").
    /// Settings come from configuration: <c>Uploads:BasePath</c>, <c>Uploads:MaxSizeBytes</c>, <c>Uploads:AllowedExtensions</c>.
    /// </summary>
    /// <exception cref="NotSupportedException">The file extension is not in the allowed list.</exception>
    /// <exception cref="ArgumentException">The file exceeds the configured maximum size.</exception>
    Task<string> SaveImageAsync(Stream imageStream, string fileName);

    /// <summary>
    /// Deletes a previously stored image. Best-effort: failures are logged and never surfaced to the caller.
    /// </summary>
    Task DeleteImageAsync(string? relativePath);
}
