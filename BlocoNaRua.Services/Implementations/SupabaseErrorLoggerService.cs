using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using BlocoNaRua.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace BlocoNaRua.Services.Implementations;

public class SupabaseErrorLoggerService(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration) : IErrorLogger
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly IConfiguration _configuration = configuration;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async Task LogAsync(ErrorLogEntry entry)
    {
        var supabaseUrl = _configuration["Supabase:Url"];
        var serviceRoleKey = _configuration["Supabase:ServiceRoleKey"];

        if (string.IsNullOrEmpty(supabaseUrl) || string.IsNullOrEmpty(serviceRoleKey))
            return;

        var client = _httpClientFactory.CreateClient("SupabaseAdmin");
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("apikey", serviceRoleKey);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", serviceRoleKey);
        client.DefaultRequestHeaders.Add("Prefer", "return=minimal");

        var payload = new
        {
            level = entry.Level.ToString(),
            source = entry.Source,
            message = entry.Message,
            stack_trace = entry.StackTrace,
            request_path = entry.RequestPath,
            request_method = entry.RequestMethod,
            status_code = entry.StatusCode,
            user_id = entry.UserId,
            additional_data = entry.AdditionalData != null ? JsonSerializer.Serialize(entry.AdditionalData, JsonOptions) : null
        };

        var content = new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json");
        await client.PostAsync($"{supabaseUrl}/rest/v1/api_errors", content);
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
