using BlocoNaRua.Data.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BlocoNaRua.Tests.Helpers;

/// <summary>
/// A simple holder for the current authenticated member's UUID.
/// Used by tests to set the current member before making authenticated requests.
/// </summary>
public class TestCurrentMemberHolder
{
    public Guid CurrentMemberSub { get; set; } = Guid.Empty;
}

public class BlocoNaRuaWebApplicationFactory : WebApplicationFactory<Program>
{
    // Fixed HS256 signing key for the tests: 32 random bytes, base64 encoded.
    private const string TestSigningKey = "dGVzdC1zaWduaW5nLWtleS1mb3ItYm5yLXRlc3RzLTMyIQ==";

    // Uploads land in a temp folder so the tests never touch the real upload volume.
    private static readonly string TestUploadsPath =
        Path.Combine(Path.GetTempPath(), "bnr-tests-uploads");

    private readonly Guid _databaseName = Guid.NewGuid();
    private readonly bool _configureGoogle;
    private TestCurrentMemberHolder? _currentMemberHolder;

    /// <param name="configureGoogle">
    /// When false the Google OAuth scheme is not registered, mirroring a deployment without
    /// Authentication__Google__ClientId / Authentication__Google__ClientSecret.
    /// </param>
    public BlocoNaRuaWebApplicationFactory(bool configureGoogle = true)
    {
        _configureGoogle = configureGoogle;
    }

    /// <summary>
    /// Factory for a deployment without Google OAuth credentials.
    /// </summary>
    public static BlocoNaRuaWebApplicationFactory WithoutGoogle() => new(configureGoogle: false);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // The app resolves these at startup (JWT validation parameters, static files).
        // Without them the host fails before any request is served.
        builder.UseSetting("Auth:SigningKey", TestSigningKey);
        builder.UseSetting("Auth:Issuer", "bloconarua-api");
        builder.UseSetting("Auth:Audience", "bloconarua-api");
        builder.UseSetting("Uploads:BasePath", TestUploadsPath);

        // The Google scheme is registered by the app only when both values are present;
        // empty values keep it out without changing any other behaviour.
        builder.UseSetting("Authentication:Google:ClientId", _configureGoogle ? "test-client" : string.Empty);
        builder.UseSetting("Authentication:Google:ClientSecret", _configureGoogle ? "test-secret" : string.Empty);

        builder.ConfigureServices(services =>
        {
            // Remove ALL database provider services (Npgsql, InMemory, etc.) and DbContext options
            var toRemove = services
                .Where(d => d.ServiceType.Namespace?.StartsWith("Microsoft.EntityFrameworkCore") == true ||
                            d.ServiceType.Namespace?.StartsWith("Npgsql.EntityFrameworkCore") == true)
                .ToList();
            foreach (var descriptor in toRemove)
            {
                services.Remove(descriptor);
            }

            // Re-add DbContext with InMemory
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName.ToString()));

            // Register the test current member holder as singleton
            _currentMemberHolder = new TestCurrentMemberHolder();
            services.AddSingleton<TestCurrentMemberHolder>(_currentMemberHolder);

            // Add TestAuth as an available authentication scheme
            services.AddAuthentication("TestAuth")
                .AddScheme<TestAuthOptions, TestAuthHandler>("TestAuth", _ => { });
        });
    }

    public void SetCurrentMember(Guid memberUuid)
    {
        if (_currentMemberHolder != null)
            _currentMemberHolder.CurrentMemberSub = memberUuid;
    }

    internal TestCurrentMemberHolder? GetCurrentMemberHolder() => _currentMemberHolder;
}
