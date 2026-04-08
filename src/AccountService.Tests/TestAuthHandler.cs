using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace AccountService.Tests;

public sealed class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "Test";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var ownerId = ResolveHeader("X-Test-User", "integration-test-user");
        var username = ResolveHeader("X-Test-Username", ownerId);
        var email = ResolveHeader("X-Test-Email", $"{ownerId}@shopisel.test");
        var name = ResolveHeader("X-Test-Name", "Integration Test User");

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, ownerId),
            new Claim("sub", ownerId),
            new Claim("preferred_username", username),
            new Claim("email", email),
            new Claim("name", name),
            new Claim("azp", "shopisel-account-api")
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    private string ResolveHeader(string headerName, string fallback)
    {
        if (!Request.Headers.TryGetValue(headerName, out StringValues values))
        {
            return fallback;
        }

        var candidate = values.ToString().Trim();
        return string.IsNullOrWhiteSpace(candidate) ? fallback : candidate;
    }
}
