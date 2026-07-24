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
    private readonly Guid _databaseName = Guid.NewGuid();
    private HttpContextAccessorStub? _httpContextAccessorStub;
    private TestCurrentMemberHolder? _currentMemberHolder;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

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

            // Replace IHttpContextAccessor with test stub
            var httpContextAccessorDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IHttpContextAccessor));
            if (httpContextAccessorDescriptor != null)
            {
                services.Remove(httpContextAccessorDescriptor);
            }

            _httpContextAccessorStub = new HttpContextAccessorStub();
            services.AddSingleton<IHttpContextAccessor>(_httpContextAccessorStub);

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

    private class HttpContextAccessorStub : IHttpContextAccessor
    {
        public HttpContext? HttpContext { get; set; }
    }
}
