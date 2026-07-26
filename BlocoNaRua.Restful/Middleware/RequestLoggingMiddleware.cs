using System.Diagnostics;

namespace BlocoNaRua.Restful.Middleware;

public class RequestLoggingMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
{
    private static readonly EventId RequestEventId = new(1, "Request");
    private readonly RequestDelegate _next = next;
    private readonly ILogger _logger = loggerFactory.CreateLogger("BlocoNaRua.Restful.Middleware");

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var method = context.Request.Method;
        var path = context.Request.Path;

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            var statusCode = context.Response.StatusCode;
            var duration = stopwatch.Elapsed.TotalMilliseconds;

            var authHeader = context.Request.Headers.Authorization.ToString();
            var hasAuth = !string.IsNullOrEmpty(authHeader);
            var authLog = hasAuth
                ? authHeader.StartsWith("Bearer ", StringComparison.Ordinal)
                    ? "Bearer [token]"
                    : authHeader
                : null;

            var contentType = context.Request.Headers.ContentType.ToString();
            var contentLength = context.Request.Headers.ContentLength.ToString();
            var statusMessage = ((System.Net.HttpStatusCode)statusCode).ToString();

            _logger.LogInformation(
                RequestEventId,
                "HTTP {Method} {Path} => {StatusCode} {StatusMessage} {DurationMs:F1}ms {Auth}{ContentType}{ContentLength}",
                method,
                path,
                statusCode,
                statusMessage,
                duration,
                hasAuth ? $"[{authLog}] " : "",
                !string.IsNullOrEmpty(contentType) ? $"[Content-Type: {contentType}] " : "",
                !string.IsNullOrEmpty(contentLength) ? $"[Content-Length: {contentLength}]" : ""
            );
        }
    }
}

public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestLoggingMiddleware>();
    }
}
