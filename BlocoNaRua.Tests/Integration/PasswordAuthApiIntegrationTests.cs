using System.Net;
using System.Text.Json;
using BlocoNaRua.Tests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BlocoNaRua.Tests.Integration;

/// <summary>
/// End to end coverage of the API owned password flow: signup creates the member and issues a
/// token pair, the second signup with the same email conflicts and login accepts the credentials.
/// </summary>
public class PasswordAuthApiIntegrationTests : IAsyncLifetime
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
    public async Task Signup_ReturnsCreated_WithTokenPair()
    {
        var response = await _client.PostAsFormUrlEncodedAsync(
            "/api/v1/auth/signup",
            new Dictionary<string, string> { ["email"] = "signup@test.com", ["password"] = "password123", ["name"] = "Signup User" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = document.RootElement;
        Assert.False(string.IsNullOrWhiteSpace(root.GetProperty("access_token").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(root.GetProperty("refresh_token").GetString()));
        Assert.Equal("bearer", root.GetProperty("token_type").GetString());
        Assert.True(root.GetProperty("expires_in").GetInt32() > 0);
    }

    [Fact]
    public async Task Signup_ReturnsConflict_WhenEmailIsAlreadyRegistered()
    {
        await _client.PostAsFormUrlEncodedAsync(
            "/api/v1/auth/signup",
            new Dictionary<string, string> { ["email"] = "duplicate@test.com", ["password"] = "password123", ["name"] = "First User" });

        var response = await _client.PostAsFormUrlEncodedAsync(
            "/api/v1/auth/signup",
            new Dictionary<string, string> { ["email"] = "duplicate@test.com", ["password"] = "password123", ["name"] = "Second User" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Signup_ReturnsBadRequest_WhenPayloadIsInvalid()
    {
        var response = await _client.PostAsFormUrlEncodedAsync(
            "/api/v1/auth/signup",
            new { email = "not-an-email", password = "short", name = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_ReturnsOk_AfterSignup()
    {
        await _client.PostAsFormUrlEncodedAsync(
            "/api/v1/auth/signup",
            new Dictionary<string, string> { ["email"] = "login@test.com", ["password"] = "password123", ["name"] = "Login User" });

        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["email"] = "login@test.com",
            ["password"] = "password123"
        });

        var response = await _client.PostAsync("/api/v1/auth/login", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenPasswordIsWrong()
    {
        await _client.PostAsFormUrlEncodedAsync(
            "/api/v1/auth/signup",
            new Dictionary<string, string> { ["email"] = "wrong@test.com", ["password"] = "password123", ["name"] = "Wrong User" });

        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["email"] = "wrong@test.com",
            ["password"] = "not-the-password"
        });

        var response = await _client.PostAsync("/api/v1/auth/login", content);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
