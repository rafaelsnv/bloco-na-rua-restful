using BlocoNaRua.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace BlocoNaRua.Services.Implementations;

public class ImageUrlService(IConfiguration configuration, IErrorLogger errorLogger) : IImageUrlService
{
    private const string DefaultBasePath = "/app/uploads";
    private const string DefaultAllowedExtensions = ".jpg,.jpeg,.png,.webp";
    private const long DefaultMaxSizeBytes = 5 * 1024 * 1024;
    private const string UrlPrefix = "/uploads";
    private const int CopyBufferSize = 81920;

    private readonly IErrorLogger _errorLogger = errorLogger;

    private readonly string _uploadDirectory =
        configuration["Uploads:BasePath"] is { Length: > 0 } basePath ? basePath : DefaultBasePath;

    private readonly long _maxSizeBytes =
        long.TryParse(configuration["Uploads:MaxSizeBytes"], out var maxSize) && maxSize > 0
            ? maxSize
            : DefaultMaxSizeBytes;

    private readonly HashSet<string> _allowedExtensions = ParseExtensions(
        configuration["Uploads:AllowedExtensions"] ?? DefaultAllowedExtensions);

    public async Task<string> SaveImageAsync(Stream imageStream, string fileName)
    {
        ArgumentNullException.ThrowIfNull(imageStream);

        // Only the extension of the caller supplied name is used; the stored name is always built here,
        // so caller supplied path segments can never reach the file system.
        var extension = Path.GetExtension(fileName ?? string.Empty);
        if (string.IsNullOrEmpty(extension) || !_allowedExtensions.Contains(extension))
        {
            throw new NotSupportedException(
                $"File extension '{extension}' is not supported. Allowed extensions: {string.Join(", ", _allowedExtensions)}.");
        }

        // Seekable streams know their size upfront, so oversized files are rejected before any disk write.
        if (imageStream.CanSeek && imageStream.Length - imageStream.Position > _maxSizeBytes)
        {
            throw new ArgumentException(
                $"File exceeds the maximum allowed size of {_maxSizeBytes} bytes.", nameof(imageStream));
        }

        var storedName = $"{Guid.NewGuid():N}{extension}";
        var targetPath = Path.Combine(_uploadDirectory, storedName);

        Directory.CreateDirectory(_uploadDirectory);

        try
        {
            await using var target = File.Create(targetPath);
            await CopyAsync(imageStream, target);
        }
        catch
        {
            TryDelete(targetPath);
            throw;
        }

        return $"{UrlPrefix}/{storedName}";
    }

    public async Task DeleteImageAsync(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return;
        }

        // Path.GetFileName keeps the lookup inside the upload directory (no traversal).
        var fileName = Path.GetFileName(relativePath);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return;
        }

        var targetPath = Path.Combine(_uploadDirectory, fileName);

        try
        {
            if (File.Exists(targetPath))
            {
                File.Delete(targetPath);
            }
        }
        catch (Exception ex)
        {
            await LogFailureAsync(ex, targetPath);
        }
    }

    private static HashSet<string> ParseExtensions(string raw)
    {
        var extensions = raw
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // An empty configuration must not silently reject every upload.
        return extensions.Count > 0
            ? extensions
            : DefaultAllowedExtensions
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private async Task CopyAsync(Stream source, Stream target)
    {
        if (source.CanSeek)
        {
            await source.CopyToAsync(target);
            return;
        }

        // Non seekable stream: enforce the limit while writing instead of buffering it in memory.
        var buffer = new byte[CopyBufferSize];
        long total = 0;
        int read;
        while ((read = await source.ReadAsync(buffer)) > 0)
        {
            total += read;
            if (total > _maxSizeBytes)
            {
                throw new ArgumentException(
                    $"File exceeds the maximum allowed size of {_maxSizeBytes} bytes.", nameof(source));
            }

            await target.WriteAsync(buffer.AsMemory(0, read));
        }
    }

    private void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch
        {
            // Best-effort cleanup, the original error is the meaningful one.
        }
    }

    private async Task LogFailureAsync(Exception exception, string path)
    {
        try
        {
            await _errorLogger.LogErrorAsync(
                nameof(ImageUrlService), $"Failed to delete image '{path}'.", exception);
        }
        catch
        {
            // Logging must never break the caller.
        }
    }
}
