namespace BlocoNaRua.Services.Interfaces;

public enum ErrorLevel
{
    Warning,
    Error,
    Critical
}

public class ErrorLogEntry
{
    public ErrorLevel Level { get; set; }
    public string Source { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public string? RequestPath { get; set; }
    public string? RequestMethod { get; set; }
    public int? StatusCode { get; set; }
    public string? UserId { get; set; }
    public object? AdditionalData { get; set; }
}

public interface IErrorLogger
{
    Task LogAsync(ErrorLogEntry entry);
    Task LogWarningAsync(string source, string message, object? additionalData = null);
    Task LogErrorAsync(string source, string message, Exception? exception = null, object? additionalData = null);
    Task LogCriticalAsync(string source, string message, Exception? exception = null, object? additionalData = null);
}
