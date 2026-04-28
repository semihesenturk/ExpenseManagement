using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Expense.IntegrationTests.Setup;

public class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Header'dan dinamik değerleri oku, yoksa varsayılan kullan
        var tenantId = Request.Headers["X-Test-TenantId"].FirstOrDefault() ?? Guid.NewGuid().ToString();
        var userId = Request.Headers["X-Test-UserId"].FirstOrDefault() ?? Guid.NewGuid().ToString();
        var role = Request.Headers["X-Test-Role"].FirstOrDefault() ?? "HR";

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim("TenantId", tenantId),
            new Claim(ClaimTypes.Role, role),
            new Claim("UserId", userId)
        };

        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "TestScheme");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}