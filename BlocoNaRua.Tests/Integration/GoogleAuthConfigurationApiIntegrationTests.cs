using System.Net;
using BlocoNaRua.Tests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BlocoNaRua.Tests.Integration;

/// <summary>
/// The Google OAuth scheme is registered by the app only when
/// Authentication__Google__ClientId / Authentication__Google__ClientSecret are configured.
/// These tests cover both sides of that switch.
/// </summary>
public class GoogleAuthConfigurationApiIntegrationTests : IAsyncLifetime
{
    private readonly BlocoNaRuaWebApplicationFactory _factory;
    private HttpClient _client = null!;

    public GoogleAuthConfigurationApiIntegrationTests()
    {
        _factory = BlocoNaRuaWebApplicationFactory.WithoutGoogle();
    }

    public Task InitializeAsync()
    {
        // No auto redirect: a redirect to accounts.google.com would leave the test server.
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
    public async Task PublicEndpoint_ReturnsSuccess_WhenGoogleIsNotConfigured()
    {
        var response = await _client.GetAsync("/");

        Assert.True(
            response.StatusCode == HttpStatusCode.OK,
            $"Expected a successful public response, got {(int)response.StatusCode}.");
    }

    [Fact]
    public async Task GoogleChallenge_ReturnsServiceUnavailable_WhenGoogleIsNotConfigured()
    {
        var response = await _client.GetAsync("/api/v1/auth/google/challenge");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }
}

/// <summary>
/// The other side of the switch: with Google credentials configured the scheme exists and
/// the challenge endpoint must still start the OAuth flow.
/// </summary>
public class GoogleAuthConfiguredApiIntegrationTests : IAsyncLifetime
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
    public async Task GoogleChallenge_RedirectsToGoogle_WhenGoogleIsConfigured()
    {
        var response = await _client.GetAsync("/api/v1/auth/google/challenge");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("accounts.google.com", response.Headers.Location?.ToString());
    }
}
