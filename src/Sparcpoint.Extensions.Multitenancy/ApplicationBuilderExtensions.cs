using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Sparcpoint.Extensions.Multitenancy;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseMultitenancy<TTenant>(this IApplicationBuilder app)
    {
        Ensure.ArgumentNotNull(app);
        return app.UseMiddleware<TenantResolutionMiddleware<TTenant>>();
    }
}
