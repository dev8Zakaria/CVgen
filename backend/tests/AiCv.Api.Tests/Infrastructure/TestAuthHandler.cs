using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AiCv.Api.Tests.Infrastructure;

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Test";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("X-Test-Auth", out var enabled) || enabled != "true")
        {
            return Task.FromResult(AuthenticateResult.Fail("Missing test auth header."));
        }

        var keycloakId = Request.Headers["X-Test-Subject"].FirstOrDefault() ?? "test-subject";
        var email = Request.Headers["X-Test-Email"].FirstOrDefault() ?? "test@example.com";
        var name = Request.Headers["X-Test-Name"].FirstOrDefault() ?? "Test User";

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, keycloakId),
            new Claim(ClaimTypes.Email, email),
            new Claim("name", name),
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
