using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BlocoNaRua.Tests.Helpers;

public class TestAuthOptions : AuthenticationSchemeOptions
{
    public Guid Sub { get; set; } = Guid.NewGuid();
}

public class TestAuthHandler : AuthenticationHandler<TestAuthOptions>
{
    public TestAuthHandler(
        IOptionsMonitor<TestAuthOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder
    ) : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // First try to get from the test current member holder (set by SetCurrentMember)
        var holder = Context.RequestServices.GetService(typeof(TestCurrentMemberHolder)) as TestCurrentMemberHolder;
        var sub = holder?.CurrentMemberSub ?? Guid.Empty;

        // Fallback to Options.Sub if not set or empty
        if (sub == Guid.Empty)
        {
            sub = Options.Sub;
        }

        var claims = new[]
        {
            new Claim("sub", sub.ToString())
        };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
