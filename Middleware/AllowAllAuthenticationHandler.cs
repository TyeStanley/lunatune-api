using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Lunatune.Middleware;

public class AllowAllAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
  protected override Task<AuthenticateResult> HandleAuthenticateAsync()
  {
    var claims = new[] { new Claim(ClaimTypes.Name, "DevUser") };
    var identity = new ClaimsIdentity(claims, "AllowAll");
    var principal = new ClaimsPrincipal(identity);
    var ticket = new AuthenticationTicket(principal, "AllowAll");
    return Task.FromResult(AuthenticateResult.Success(ticket));
  }
}