using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Sparcpoint.Extensions.Multitenancy;

public class CustomAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly CustomAuthenticationHandlerOptions _DynamicOptions;

    public CustomAuthenticationHandler(CustomAuthenticationHandlerOptions dynamicOptions, IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) 
        : base(options, logger, encoder, clock)
    {
        _DynamicOptions = dynamicOptions;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (_DynamicOptions.IsAuthenticated && !string.IsNullOrWhiteSpace(_DynamicOptions.AccountId))
        {
            var identity = new ClaimsIdentity("default", ClaimTypes.Name, ClaimTypes.Role);
            identity.AddClaim(new Claim(ClaimTypes.Name, _DynamicOptions.AccountId));
            identity.AddClaim(new Claim(ClaimTypes.Role, "Admin"));

            var principal = new ClaimsPrincipal(identity);
            return AuthenticateResult.Success(new AuthenticationTicket(principal, "default"));
        }

        return AuthenticateResult.Fail(new Exception("Not Authenticated"));
    }
}