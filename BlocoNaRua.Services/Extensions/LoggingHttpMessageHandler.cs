using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace BlocoNaRua.Services.Extensions;

public class LoggingHttpMessageHandler : DelegatingHandler
{
    private readonly ILogger<LoggingHttpMessageHandler> _logger;

    public LoggingHttpMessageHandler(ILogger<LoggingHttpMessageHandler> logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestUri = request.RequestUri?.ToString() ?? "unknown";
        var method = request.Method.Method;

        try
        {
            var response = await base.SendAsync(request, cancellationToken);
            stopwatch.Stop();

            _logger.LogInformation(
                "Supabase Auth Request: {Method} {Uri} - {StatusCode} in {ElapsedMs}ms",
                method,
                requestUri,
                (int)response.StatusCode,
                stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(
                ex,
                "Supabase Auth Request Failed: {Method} {Uri} in {ElapsedMs}ms - {Error}",
                method,
                requestUri,
                stopwatch.ElapsedMilliseconds,
                ex.Message);

            throw;
        }
    }
}
