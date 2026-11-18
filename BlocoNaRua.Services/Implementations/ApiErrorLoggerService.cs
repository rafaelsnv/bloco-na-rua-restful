using System.Text.Json;
using BlocoNaRua.Data.Context;
using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Services.Interfaces;

namespace BlocoNaRua.Services.Implementations;

public class ApiErrorLoggerService(AppDbContext appDbContext) : IErrorLogger
{
    private readonly AppDbContext _appDbContext = appDbContext;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async Task LogAsync(ErrorLogEntry entry)
    {
        try
        {
            var entity = new ApiErrorEntity(0)
            {
                Level = entry.Level.ToString(),
                Source = entry.Source,
                Message = entry.Message,
                StackTrace = entry.StackTrace,
                RequestPath = entry.RequestPath,
                RequestMethod = entry.RequestMethod,
                StatusCode = entry.StatusCode,
                UserId = entry.UserId,
                AdditionalData = entry.AdditionalData != null
                    ? JsonSerializer.Serialize(entry.AdditionalData, JsonOptions)
                    : null
            };

            _appDbContext.ApiErrors.Add(entity);
            await _appDbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Error logging must never throw or break the request pipeline.
            Console.WriteLine($"[ApiErrorLoggerService] Failed to persist error log: {ex}");
        }
    }

    public Task LogWarningAsync(string source, string message, object? additionalData = null)
        => LogAsync(new ErrorLogEntry
        {
            Level = ErrorLevel.Warning,
            Source = source,
            Message = message,
            AdditionalData = additionalData
        });

    public Task LogErrorAsync(string source, string message, Exception? exception = null, object? additionalData = null)
        => LogAsync(new ErrorLogEntry
        {
            Level = ErrorLevel.Error,
            Source = source,
            Message = message,
            StackTrace = exception?.StackTrace,
            AdditionalData = additionalData
        });

    public Task LogCriticalAsync(string source, string message, Exception? exception = null, object? additionalData = null)
        => LogAsync(new ErrorLogEntry
        {
            Level = ErrorLevel.Critical,
            Source = source,
            Message = message,
            StackTrace = exception?.StackTrace,
            AdditionalData = additionalData
        });
}
