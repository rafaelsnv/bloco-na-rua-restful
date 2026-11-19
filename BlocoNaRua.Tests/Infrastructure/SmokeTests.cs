using System.Net;
using System.Text.Json;
using BlocoNaRua.Tests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BlocoNaRua.Tests.Infrastructure;

/// <summary>
/// Startup-level smoke tests: they cover the failures unit tests cannot see because they
/// only happen while the app is serving requests (Swashbuckle operation generation) or while
/// the host boots without a secret (Auth:SigningKey).
/// </summary>
public class SwaggerSmokeTests : IAsyncLifetime
{
    private readonly BlocoNaRuaWebApplicationFactory _factory = new();
    private HttpClient _client = null!;

    public Task InitializeAsync()
    {
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _client.Dispose();
        _factory.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task SwaggerDocument_ReturnsOk_WithNonEmptyJsonBody()
    {
        var response = await _client.GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.False(string.IsNullOrWhiteSpace(body), "The swagger document body must not be empty.");

        using var document = JsonDocument.Parse(body);
        Assert.True(
            document.RootElement.TryGetProperty("paths", out _),
            "The swagger document must contain a 'paths' section.");
    }
}

/// <summary>
/// A deployment without Auth:SigningKey must not take the API down: Program.cs falls back to a
/// random ephemeral key so the host boots and public routes keep answering.
/// </summary>
public class MissingSigningKeySmokeTests : IAsyncLifetime
{
    private readonly BlocoNaRuaWebApplicationFactory _factory =
        BlocoNaRuaWebApplicationFactory.WithoutSigningKey();
    private HttpClient _client = null!;

    public Task InitializeAsync()
    {
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _client.Dispose();
        _factory.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task PublicRoute_ReturnsHandledStatus_WhenSigningKeyIsMissing()
    {
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["email"] = "smoke@test.com",
            ["password"] = "smoke-password"
        });

        var response = await _client.PostAsync("/api/v1/auth/login", content);

        Assert.NotEqual(
            HttpStatusCode.InternalServerError,
            response.StatusCode);
        Assert.NotEqual(
            HttpStatusCode.NotFound,
            response.StatusCode);
        // The credentials are unknown: the login answers 401 and never touches the signing key.
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }
}
